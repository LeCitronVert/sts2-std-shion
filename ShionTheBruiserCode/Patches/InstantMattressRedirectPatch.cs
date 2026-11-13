using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
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
#if STS2_BETA
// Beta: the funnel takes per-target data (Creature + hit VFX), and attacks call it directly.
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    [typeof(PlayerChoiceContext), typeof(IEnumerable<DamageTargetData>), typeof(decimal), typeof(ValueProp), typeof(Creature),
        typeof(CardModel), typeof(CardPlay), typeof(ModifyDamageHookType)])]
#else
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    [typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)])]
#endif
public static class InstantMattressRedirectPatch
{
    [HarmonyPrefix]
#if STS2_BETA
    public static void Prefix(ref IEnumerable<DamageTargetData>? targetData, ValueProp props, Creature? dealer)
    {
        if (dealer is not { IsEnemy: true } || !props.IsPoweredAttack() || targetData is null) return;

        var dataList = targetData.ToList();
        var targets = dataList.Select(d => d.target);
        if (Redirect(ref targets)) targetData = dataList.Zip(targets, (d, t) => d with { target = t }).ToList();
    }
#else
    public static void Prefix(ref IEnumerable<Creature> targets, ValueProp props, Creature? dealer)
    {
        if (dealer is not { IsEnemy: true } || !props.IsPoweredAttack()) return;
        Redirect(ref targets);
    }
#endif

    /// <summary>Hits on other players go to the first Mattress owner. True if any target changed.</summary>
    private static bool Redirect(ref IEnumerable<Creature> targets)
    {
        var targetList = targets.ToList();
        var combatState = targetList.FirstOrDefault()?.CombatState;
        if (combatState is null) return false;

        var mattresses = combatState.Allies.Where(c => c.IsAlive && c.HasPower<InstantMattress>()).ToList();
        if (mattresses.Count == 0) return false;

        var redirected = false;
        for (var i = 0; i < targetList.Count; i++)
        {
            var target = targetList[i];
            if (!target.IsPlayer || target.HasPower<InstantMattress>()) continue;

            targetList[i] = mattresses[0];
            redirected = true;
        }

        if (redirected) targets = targetList;
        return redirected;
    }
}
