using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Summon Wrath: the base damage of the next X Attacks you play this turn is multiplied by the number of ALL identical
/// cards you have (same card, whatever its pile, upgrade or enchantment; the played card counts), and playing it makes
/// you lose that same number of HP.
/// <para>
/// "Base damage" includes the card's enchantment but not Strength and other modifiers: the multiplier is applied in
/// <see cref="ModifyDamagePatch"/>, right after the enchantment and before the additive hooks.
/// While the power is active, Attacks in your Hand preview the multiplied damage, and <see cref="DescriptionPatch"/>
/// adds the HP they'd cost to their text.
/// </para>
/// </summary>
public class SummonWrath : SeelsTheRecklessPower
{
    private class Data
    {
        /// <summary>The Attack being played with the multiplier, and its multiplier (counted when it started).</summary>
        public CardModel? wrathedCard;
        public int multiplier = 1;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    /// <summary>ALL copies of this card you have (any pile), the card itself included.</summary>
    public static int CountIdentical(CardModel card)
    {
        var count = SeelsMechanicsHelper.GetAllCombatCards(card.Owner).Count(c => c.Id == card.Id);
        return Math.Max(1, count);
    }

    /// <summary>
    /// The multiplier this card's base damage gets right now, or null if Summon Wrath doesn't affect it.
    /// Pure (used by previews): the card being played keeps the multiplier counted when it started;
    /// otherwise, Attacks in your Hand preview the multiplier they would get if played next.
    /// </summary>
    public static int? GetMultiplier(CardModel card)
    {
        if (card.Type != CardType.Attack || card.CombatState is null) return null;
        var power = card.Owner?.Creature.GetPower<SummonWrath>();
        if (power is null) return null;

        var data = power.GetInternalData<Data>();
        if (data.wrathedCard is not null)
        {
            return data.wrathedCard == card ? data.multiplier : null;
        }
        if (power.Amount <= 0 || card.Pile?.Type != PileType.Hand) return null;
        return CountIdentical(card);
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var data = GetInternalData<Data>();
        if (card.Owner.Creature != Owner || card.Type != CardType.Attack) return;
        if (data.wrathedCard is not null || cardPlay.PlayIndex != 0 || Amount <= 0) return;

        data.wrathedCard = card;
        data.multiplier = CountIdentical(card);

        Flash();
        // No choice context in this hook; losing HP never asks the player anything.
        await SeelsMechanicsHelper.LoseHp(new ThrowingPlayerChoiceContext(), Owner, data.multiplier, null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        if (cardPlay.Card != data.wrathedCard || cardPlay.PlayIndex != cardPlay.PlayCount - 1) return;

        data.wrathedCard = null;
        data.multiplier = 1;
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }

    /// <summary>
    /// Multiplies the base damage (after the card's enchantment, before Strength etc.) of Summon Wrath's Attacks.
    /// Covers real damage and every preview, since they all go through Hook.ModifyDamage.
    /// </summary>
    [HarmonyPatch(typeof(Hook), "ModifyDamageInternal")]
    public static class ModifyDamagePatch
    {
        [HarmonyPrefix]
        public static void Prefix(Creature? dealer, ref decimal damage, ValueProp props, CardModel? cardSource, ModifyDamageHookType modifyDamageHookType)
        {
            // Only on the pass that starts from the base damage (a multiplicative-only pass gets the already modified amount)
            if (!modifyDamageHookType.HasFlag(ModifyDamageHookType.Additive)) return;
            if (cardSource is null || !props.IsPoweredAttack() || dealer != cardSource.Owner?.Creature) return;
            if (GetMultiplier(cardSource) is { } multiplier && multiplier > 1)
            {
                damage *= multiplier;
            }
        }
    }

    /// <summary>Adds "Summon Wrath: lose X HP." to the Attacks Summon Wrath would affect.</summary>
    [HarmonyPatch]
    public static class DescriptionPatch
    {
        private static System.Reflection.MethodBase TargetMethod() =>
            AccessTools.GetDeclaredMethods(typeof(CardModel))
                .First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

        [HarmonyPostfix]
        public static void Postfix(CardModel __instance, PileType pileType, ref string __result)
        {
            if (pileType is not (PileType.Hand or PileType.Play)) return;
            if (GetMultiplier(__instance) is not { } multiplier) return;

            var line = new LocString("powers", "SHIONTHEBRUISER-SUMMON_WRATH.cardText");
            line.Add("Amount", multiplier);
            __result += "\n" + line.GetFormattedText();
        }
    }
}
