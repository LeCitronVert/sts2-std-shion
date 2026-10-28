using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Unlocks;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

/// <summary>
/// Access to the game's private epoch helpers, so custom epochs are obtained exactly like the base ones
/// (same VFX, same "epochs locked" checks for custom/daily modes, same DiscoveredEpochs bookkeeping).
/// </summary>
public static class EpochProgress
{
    private static readonly Func<ProgressSaveManager, EpochModel, Player, bool> TryObtainEpochMidRun =
        AccessTools.MethodDelegate<Func<ProgressSaveManager, EpochModel, Player, bool>>(
            AccessTools.Method(typeof(ProgressSaveManager), "TryObtainEpochMidRun"));

    private static readonly Func<ProgressSaveManager, EpochModel, SerializablePlayer, SerializableRun, bool> TryObtainEpochPostRun =
        AccessTools.MethodDelegate<Func<ProgressSaveManager, EpochModel, SerializablePlayer, SerializableRun, bool>>(
            AccessTools.Method(typeof(ProgressSaveManager), "TryObtainEpochPostRun"));

    private static readonly Func<CharacterModel, CharacterModel?> UnlocksAfterRunAsGetter =
        AccessTools.MethodDelegate<Func<CharacterModel, CharacterModel?>>(
            AccessTools.PropertyGetter(typeof(CharacterModel), "UnlocksAfterRunAs"));

    private static readonly AccessTools.FieldRef<UnlockState, HashSet<string>> UnlockedEpochIds =
        AccessTools.FieldRefAccess<UnlockState, HashSet<string>>("_unlockedEpochIds");

    /// <summary>
    /// Obtains the epoch during a run (boss/elite kills). False if already obtained or epochs are locked for this mode.
    /// </summary>
    public static bool ObtainMidRun(ProgressSaveManager manager, EpochModel epoch, Player localPlayer) =>
        TryObtainEpochMidRun(manager, epoch, localPlayer);

    /// <summary>
    /// Obtains the epoch at the end of a run. False if already obtained or epochs are locked for this mode.
    /// </summary>
    public static bool ObtainPostRun(ProgressSaveManager manager, EpochModel epoch, SerializablePlayer player, SerializableRun run) =>
        TryObtainEpochPostRun(manager, epoch, player, run);

    /// <summary>
    /// The character a run must be played with to unlock this one (the protected <c>CharacterModel.UnlocksAfterRunAs</c>).
    /// </summary>
    public static CharacterModel? UnlocksAfterRunAs(CharacterModel character) => UnlocksAfterRunAsGetter(character);

    /// <summary>
    /// <see cref="UnlockState.IsEpochRevealed{T}"/> by id. In multiplayer, the shared state holds every player's epochs.
    /// </summary>
    public static bool IsRevealed(UnlockState unlockState, string epochId) => UnlockedEpochIds(unlockState).Contains(epochId);

    /// <summary>
    /// True if the local profile has finished (won or lost) at least one run with this character.
    /// </summary>
    public static bool HasPlayedRunAs(CharacterModel character)
    {
        var stats = SaveManager.Instance.Progress.GetStatsForCharacter(character.Id);
        return stats is not null && stats.TotalWins + stats.TotalLosses > 0;
    }
}
