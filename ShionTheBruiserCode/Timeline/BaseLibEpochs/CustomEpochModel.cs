using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

/// <summary>
/// In-mod copy of BaseLib PR #331's <c>BaseLib.Abstracts.CustomEpochModel</c> (same members, same behavior).
/// Not stored in ModelDb: registered by <see cref="CustomEpochRegistry"/>.
/// See <see cref="CustomEpochRegistry"/> for how to switch to BaseLib's version.
/// </summary>
public abstract class CustomEpochModel : EpochModel
{
    /// <summary>
    /// Mod prefix + slugified class name, e.g. SHIONTHEBRUISER-SHION_THE_CALL_EPOCH.
    /// </summary>
    public override string Id => $"{GetType().GetPrefix()}{StringHelper.Slugify(GetType().Name).ToUpperInvariant()}";

    /// <summary>
    /// The large image displayed when clicking the Epoch.
    /// </summary>
    public abstract string CustomRealPortraitPath { get; }

    /// <summary>
    /// The small image displayed on the Timeline.
    /// </summary>
    public abstract string CustomPackedPortraitPath { get; }

    /// <summary>
    /// Set to true if your epoch unlocks cards so it can be added to SaveManager.GetCardUnlockEpochIds.
    /// </summary>
    protected virtual bool UnlocksCards => false;

    internal bool UnlocksCardsInternal => UnlocksCards;

    [HarmonyPatch]
    private static class PropertyRedirections
    {
        [HarmonyPatch(typeof(EpochModel), "RealPortraitPath", MethodType.Getter)]
        [HarmonyPrefix]
        private static bool CustomEpochRealPortraitPath(EpochModel __instance, ref string? __result)
        {
            if (__instance is not CustomEpochModel customEpoch) return true;
            __result = customEpoch.CustomRealPortraitPath;
            return false;
        }

        [HarmonyPatch(typeof(EpochModel), nameof(PackedPortraitPath), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool CustomEpochPackedPortraitPath(EpochModel __instance, ref string? __result)
        {
            if (__instance is not CustomEpochModel customEpoch) return true;
            __result = customEpoch.CustomPackedPortraitPath;
            return false;
        }
    }
}
