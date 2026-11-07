using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Ancients;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer). Whenever you Mend someone, heal 15% of your Max HP.
/// </summary>
public class LeafRobe : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Percent", 15m)];

    internal async Task AfterMend()
    {
        Flash();
        await CreatureCmd.Heal(Owner.Creature, Math.Max(1m, Math.Floor(Owner.Creature.MaxHp * DynamicVars["Percent"].BaseValue / 100m)));
    }
}

/// <summary>Mend runs on every client (its target is a synced choice), so the heal does too.</summary>
[HarmonyPatch(typeof(MendRestSiteOption), nameof(MendRestSiteOption.OnSelect))]
public static class LeafRobeMendPatch
{
    [HarmonyPostfix]
    public static void Postfix(MendRestSiteOption __instance, ref Task<bool> __result) => __result = After(__result, __instance);

    private static async Task<bool> After(Task<bool> original, MendRestSiteOption option)
    {
        var mended = await original;
        if (!mended) return false;
        foreach (var robe in CosmoAccess.OwnerOf(option).Relics.OfType<LeafRobe>())
        {
            await robe.AfterMend();
        }
        return true;
    }
}
