using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

[HarmonyPatch]
public static class DreamersEpochPatches
{
    /// <summary>
    /// Custom unlock conditions (<see cref="DreamersEpoch.IsEarnedPostRun"/>), checked after the base post-run epochs.
    /// </summary>
    [HarmonyPatch(typeof(ProgressSaveManager), "UpdateEpochsPostRun")]
    [HarmonyPostfix]
    private static void ObtainCustomPostRunEpochs(ProgressSaveManager __instance, SerializablePlayer serializablePlayer, SerializableRun serializableRun, bool victory)
    {
        foreach (var epoch in DreamersEpoch.All)
        {
            if (!epoch.IsEarnedPostRun(serializablePlayer, serializableRun, victory)) continue;
            if (EpochProgress.ObtainPostRun(__instance, epoch, serializablePlayer, serializableRun))
                MainFile.Logger.Info($"Epoch {epoch.Id} obtained post-run");
        }
    }

    private static bool _validated;

    [HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
    [HarmonyPostfix]
    private static void ValidateOnce()
    {
        if (_validated) return;
        _validated = true;
        EpochValidator.Run();
    }
}
