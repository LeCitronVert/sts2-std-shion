using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// ALL Attacks hit a number of times equal to the energy paid for them + X, and gain Erratic when played.
/// <para>
/// "Energy paid" is what the play actually spent: X for X-cost cards, 0 for free or autoplayed cards.
/// The hit count of every attack command the card makes is overwritten (Twin Strike played for 1 hits 2 times,
/// not 4). Damage that isn't an attack command (HP loss, thorns...) isn't affected.
/// While the power is active, Attacks in your Hand show how many times they would hit (<see cref="DescriptionPatch"/>).
/// </para>
/// </summary>
public class UntelegraphAttacks : SeelsTheRecklessPower
{
    private class Data
    {
        /// <summary>Energy paid by each Attack currently being played.</summary>
        public readonly Dictionary<CardModel, int> energyPaid = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic)];

    protected override object InitInternalData() => new Data();

    private bool IsOwnAttack(CardModel card) => card.Type == CardType.Attack && card.Owner?.Creature == Owner;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (IsOwnAttack(cardPlay.Card))
        {
            GetInternalData<Data>().energyPaid[cardPlay.Card] = cardPlay.Resources.EnergySpent;
        }
        return Task.CompletedTask;
    }

    public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
    {
        if (attack.ModelSource is not CardModel card || attack.Attacker != Owner) return hitCount;
        return GetInternalData<Data>().energyPaid.TryGetValue(card, out var paid) ? paid + Amount : hitCount;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsOwnAttack(cardPlay.Card)) return Task.CompletedTask;

        if (cardPlay.PlayIndex == cardPlay.PlayCount - 1)
        {
            GetInternalData<Data>().energyPaid.Remove(cardPlay.Card);
        }
        if (!SeelsMechanicsHelper.IsErratic(cardPlay.Card))
        {
            Flash();
            SeelsMechanicsHelper.AddErratic(cardPlay.Card);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Hits an Attack in the Hand would get if played now (null if the power doesn't affect it).
    /// Pure: used by the card text.
    /// </summary>
    private static int? PreviewHitCount(CardModel card)
    {
        if (card.Type != CardType.Attack || card.CombatState is null || card.Pile?.Type != PileType.Hand) return null;
        var amount = card.Owner?.Creature.GetPowerAmount<UntelegraphAttacks>() ?? 0;
        if (amount <= 0) return null;

        var paid = card.EnergyCost.CostsX
            ? card.Owner!.PlayerCombatState?.Energy ?? 0
            : Math.Max(0, card.EnergyCost.GetResolved());
        return paid + amount;
    }

    /// <summary>Adds "Untelegraph Attacks: hits N times." to the Attacks in your Hand.</summary>
    [HarmonyPatch]
    public static class DescriptionPatch
    {
        private static System.Reflection.MethodBase TargetMethod() =>
            AccessTools.GetDeclaredMethods(typeof(CardModel))
                .First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

        [HarmonyPostfix]
        public static void Postfix(CardModel __instance, PileType pileType, ref string __result)
        {
            if (pileType != PileType.Hand) return;
            if (PreviewHitCount(__instance) is not { } hits) return;

            var line = new LocString("powers", "SHIONTHEBRUISER-UNTELEGRAPH_ATTACKS.cardText");
            line.Add("Amount", hits);
            __result += "\n" + line.GetFormattedText();
        }
    }
}
