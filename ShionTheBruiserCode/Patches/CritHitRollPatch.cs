using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Rolls the per-hit crit right before real damage is dealt.
/// Every CreatureCmd.Damage overload funnels into this one, and its prefix runs synchronously
/// before the damage calculation, so this is the only deterministic per-hit point:
/// ModifyDamageMultiplicative is also called by card previews, which differ per client.
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
public static class CritHitRollPatch
{
    [HarmonyPrefix]
#if STS2_BETA
    public static void Prefix(IEnumerable<DamageTargetData>? targetData, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (targetData is null) return;
        ShionCritSingleton.Instance?.RollPendingHit(dealer, targetData.Select(d => d.target), props, cardSource);
    }
#else
    public static void Prefix(IEnumerable<Creature> targets, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        ShionCritSingleton.Instance?.RollPendingHit(dealer, targets, props, cardSource);
    }
#endif
}
