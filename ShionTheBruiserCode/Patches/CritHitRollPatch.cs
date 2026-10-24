using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    [typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)])]
public static class CritHitRollPatch
{
    [HarmonyPrefix]
    public static void Prefix(ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        ShionCritSingleton.Instance?.RollPendingHit(dealer, props, cardSource);
    }
}
