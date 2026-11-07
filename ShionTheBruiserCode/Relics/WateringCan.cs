using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Ancients;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. The first time you Smith an unenchanted card at a Rest Site, Enchant it with Sown.
/// </summary>
public class WateringCan : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Sown>();

    private bool _wasUsed;

    public override bool IsUsedUp => _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (_wasUsed) Status = RelicStatus.Disabled;
        }
    }

    internal void AfterSmith(IEnumerable<CardModel> smithed)
    {
        if (WasUsed) return;
        var sown = ModelDb.Enchantment<Sown>();
        var card = smithed.FirstOrDefault(c => c.Enchantment == null && sown.CanEnchant(c));
        if (card == null) return;

        Flash();
        CardCmd.Enchant<Sown>(card, 1m);
        WasUsed = true;
    }
}

[HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.OnSelect))]
public static class WateringCanSmithPatch
{
    [HarmonyPostfix]
    public static void Postfix(SmithRestSiteOption __instance, ref Task<bool> __result) => __result = After(__result, __instance);

    private static async Task<bool> After(Task<bool> original, SmithRestSiteOption option)
    {
        if (!await original) return false;
        var smithed = CosmoAccess.SmithSelection(option)?.ToList() ?? [];
        foreach (var can in CosmoAccess.OwnerOf(option).Relics.OfType<WateringCan>())
        {
            can.AfterSmith(smithed);
        }
        return true;
    }
}
