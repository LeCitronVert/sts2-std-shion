using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Instant Mattress: enemy Attack damage aimed at another player hits the Mattress owner instead.
/// Done on the targets list (every CreatureCmd.Damage overload funnels into this one), so the owner's Block
/// absorbs it, unlike the game's ModifyUnblockedDamageTarget which only moves unblocked damage.
/// Runs on every client in the same order, so the redirect is deterministic.
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    [typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)])]
public static class InstantMattressRedirectPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref IEnumerable<Creature> targets, ValueProp props, Creature? dealer)
    {
        if (dealer is not { IsEnemy: true } || !props.IsPoweredAttack()) return;

        var targetList = targets.ToList();
        var combatState = targetList.FirstOrDefault()?.CombatState;
        if (combatState is null) return;

        var mattresses = combatState.Allies.Where(c => c.IsAlive && c.HasPower<InstantMattress>()).ToList();
        if (mattresses.Count == 0) return;

        var redirected = false;
        for (var i = 0; i < targetList.Count; i++)
        {
            var target = targetList[i];
            if (!target.IsPlayer || target.HasPower<InstantMattress>()) continue;

            targetList[i] = mattresses[0];
            redirected = true;
        }

        if (redirected) targets = targetList;
    }
}
