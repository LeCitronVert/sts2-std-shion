using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// The base game's <see cref="Inky"/> applies Weak to <c>cardPlay.Target</c> unless the card targets
/// All Enemies. Random-target cards (Inky Slashes from Pin Them Down) have no Target, so it threw a
/// NullReferenceException that aborted the card play: the card hung mid-screen and no Weak was applied.
/// For those, apply Weak to the enemies the card actually hit during this play instead.
/// </summary>
[HarmonyPatch(typeof(Inky), nameof(Inky.OnPlay))]
public static class InkyRandomTargetPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Inky __instance, PlayerChoiceContext choiceContext, CardPlay? cardPlay, ref Task __result)
    {
        var card = __instance.Card;
        if (cardPlay?.Target != null || card.TargetType == TargetType.AllEnemies) return true;

        __result = ApplyToHitEnemies(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task ApplyToHitEnemies(Inky inky, PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (cardPlay is null) return;
        var card = inky.Card;
        var targets = GetEnemiesHitThisPlay(card, cardPlay);
        if (targets.Count == 0) return;

        await PowerCmd.Apply<WeakPower>(choiceContext, targets, inky.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
    }

    /// <summary>
    /// Enemies damaged by this card since this specific play started (deterministic: read from combat history).
    /// </summary>
    private static List<Creature> GetEnemiesHitThisPlay(CardModel card, CardPlay cardPlay)
    {
        var ownerSide = card.Owner.Creature.Side;
        var hit = new List<Creature>();
        foreach (var entry in CombatManager.Instance.History.Entries.Reverse())
        {
            if (entry is CardPlayStartedEntry started && started.CardPlay == cardPlay) break;
            if (entry is DamageReceivedEntry damage
                && damage.CardSource == card
                && damage.Receiver.Side != ownerSide
                && damage.Receiver.IsAlive
                && !hit.Contains(damage.Receiver))
            {
                hit.Add(damage.Receiver);
            }
        }
        hit.Reverse(); // keep hit order
        return hit;
    }
}
