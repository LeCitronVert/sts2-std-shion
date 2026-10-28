using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

/// <summary>
/// In-mod copy of BaseLib PR #331's character epoch logic (CustomCharacterModel patches + CharacterEpochsUnlockPatches):
/// locks characters behind their <see cref="IEpochCharacter.UnlockEpoch"/> and obtains their epochs on the same triggers
/// as the base characters.
///
/// Deliberate differences with the PR (worth reporting upstream before switching):
///  - Retroactive unlock on an existing save requires a finished run with the prerequisite character
///    (the PR checks whether the prerequisite's own unlock epoch is revealed, which only means it is unlocked).
///    The epoch gets a slot only if its parent epoch is already revealed, otherwise it waits for the parent's expansion.
///  - Post-run unlock: the PR's filter <c>is not null or Ironclad</c> reads as "(not null) or Ironclad".
///  - Prefixes run first: BaseLib 3.4.x already skips these methods for every custom character, and Harmony
///    doesn't run a bool prefix once an earlier one returned false. (The PR removes BaseLib's skips instead.)
/// </summary>
[HarmonyPatch]
public static class CharacterEpochPatches
{
    private static IEnumerable<(CharacterModel Character, IEpochCharacter Epochs)> EpochCharacters =>
        ModelDb.AllCharacters.OfType<IEpochCharacter>().Select(e => ((CharacterModel)e, e));

    /// <summary>
    /// Unlock epochs of the base characters. Their expansion reveals the slot of characters unlocked after them.
    /// </summary>
    private static readonly Dictionary<Type, Type> BaseUnlockEpochs = new()
    {
        [typeof(Ironclad)] = typeof(NeowEpoch), // Ironclad has no unlock epoch: Neow plays that role (like for the Silent)
        [typeof(Silent)] = typeof(Silent1Epoch),
        [typeof(Regent)] = typeof(Regent1Epoch),
        [typeof(Necrobinder)] = typeof(Necrobinder1Epoch),
        [typeof(Defect)] = typeof(Defect1Epoch),
    };

    private static bool UnlocksAfterNeow(CharacterModel character) =>
        EpochProgress.UnlocksAfterRunAs(character) is null or Ironclad;

    /// <summary>
    /// The epoch whose timeline expansion reveals the slot of <paramref name="character"/>'s unlock epoch.
    /// </summary>
    private static string? ParentEpochId(CharacterModel character)
    {
        var prerequisite = EpochProgress.UnlocksAfterRunAs(character) ?? ModelDb.Character<Ironclad>();
        if (BaseUnlockEpochs.TryGetValue(prerequisite.GetType(), out var baseEpoch)) return EpochModel.GetId(baseEpoch);
        if (prerequisite is IEpochCharacter { UnlockEpoch: { } customEpoch }) return customEpoch.Id;

        MainFile.Logger.Warn($"{character.Id} unlocks after {prerequisite.Id}, which has no unlock epoch: its unlock epoch can't get a timeline slot.");
        return null;
    }

    [HarmonyPatch(typeof(UnlockState), nameof(UnlockState.Characters), MethodType.Getter)]
    [HarmonyPostfix]
    private static void LockCharactersWithUnlockEpoch(UnlockState __instance, ref IEnumerable<CharacterModel> __result)
    {
        var locked = EpochCharacters
            .Where(c => c.Epochs.UnlockEpoch is { } epoch && !EpochProgress.IsRevealed(__instance, epoch.Id))
            .Select(c => c.Character)
            .ToHashSet();
        if (locked.Count == 0) return;
        __result = __result.Where(c => !locked.Contains(c)).ToList();
    }

    /// <summary>
    /// Saves from before the mod (or before a character got locked) may already meet the unlock condition.
    /// Checked on every main menu load because of profile switching, deletion and importing.
    /// </summary>
    [HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
    [HarmonyPrefix]
    private static void CheckUnlockConditions()
    {
        var save = SaveManager.Instance;
        foreach (var (character, epochs) in EpochCharacters)
        {
            if (epochs.UnlockEpoch is not { } unlockEpoch || save.Progress.IsEpochObtained(unlockEpoch.Id)) continue;
            if (ParentEpochId(character) is not { } parentId) continue;

            var prerequisite = EpochProgress.UnlocksAfterRunAs(character);
            var earned = UnlocksAfterNeow(character)
                ? save.IsEpochRevealed(parentId)
                : EpochProgress.HasPlayedRunAs(prerequisite!);
            if (!earned) continue;

            // Without a revealed parent, the slot appears (already obtained) when the parent expands the timeline
            save.ObtainEpochOverride(unlockEpoch.Id, save.IsEpochRevealed(parentId) ? EpochState.Obtained : EpochState.ObtainedNoSlot);
            MainFile.Logger.Info($"Epoch {unlockEpoch.Id} obtained retroactively for {character.Id}");
        }
    }

    /// <summary>
    /// Characters unlocked after Neow (prerequisite null or Ironclad) are obtained with it, like the Silent.
    /// </summary>
    [HarmonyPatch(typeof(NeowEpoch), nameof(NeowEpoch.QueueUnlocks))]
    [HarmonyPrefix]
    private static void QueueUnlocksInsert()
    {
        foreach (var (character, epochs) in EpochCharacters)
        {
            if (epochs.UnlockEpoch is not { } unlockEpoch || !UnlocksAfterNeow(character)) continue;
            if (SaveManager.Instance.Progress.IsEpochObtained(unlockEpoch.Id)) continue;
            SaveManager.Instance.ObtainEpochOverride(unlockEpoch.Id, EpochState.ObtainedNoSlot);
        }
    }

    /// <summary>
    /// Adds the unlock epoch of characters unlocked after a base character to that character's unlock epoch expansion.
    /// Custom prerequisites are handled by the unlock epoch itself (see CharacterUnlockEpoch).
    /// </summary>
    [HarmonyPatch]
    private static class TimelineExpansionPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() =>
            BaseUnlockEpochs.Values.Select(t => AccessTools.Method(t, nameof(EpochModel.GetTimelineExpansion)));

        [HarmonyPostfix]
        private static void Postfix(MethodBase __originalMethod, ref EpochModel[] __result)
        {
            var prerequisiteType = BaseUnlockEpochs.First(p => p.Value == __originalMethod.DeclaringType).Key;
            __result =
            [
                .. __result,
                .. EpochCharacters
                    .Where(c => c.Epochs.UnlockEpoch is not null)
                    .Where(c => (EpochProgress.UnlocksAfterRunAs(c.Character) ?? ModelDb.Character<Ironclad>()).GetType() == prerequisiteType)
                    .Select(c => c.Epochs.UnlockEpoch!),
            ];
        }
    }

    [HarmonyPatch(typeof(ProgressSaveManager), "PostRunUnlockCharacterEpochCheck")]
    [HarmonyPrefix]
    private static void UnlockCustomCharacterEpoch(ProgressSaveManager __instance, SerializablePlayer serializablePlayer, SerializableRun serializableRun)
    {
        foreach (var (character, epochs) in EpochCharacters)
        {
            if (epochs.UnlockEpoch is not { } unlockEpoch || UnlocksAfterNeow(character)) continue;
            if (EpochProgress.UnlocksAfterRunAs(character)!.Id != serializablePlayer.CharacterId) continue;
            if (EpochProgress.ObtainPostRun(__instance, unlockEpoch, serializablePlayer, serializableRun))
                MainFile.Logger.Info($"Epoch {unlockEpoch.Id} obtained for playing a run as {serializablePlayer.CharacterId}.");
        }
    }

    /// <summary>
    /// The game builds "&lt;CHARACTER&gt;2/3/4_EPOCH" ids from the act index here.
    /// </summary>
    [HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool ObtainActEpoch(ProgressSaveManager __instance, Player localPlayer, int act)
    {
        if (localPlayer.Character is not IEpochCharacter epochs) return true;
        if (!localPlayer.Character.IsPlayable) return false;

        var epoch = act switch
        {
            0 => epochs.Act1Epoch,
            1 => epochs.Act2Epoch,
            2 => epochs.Act3Epoch,
            _ => null,
        };
        if (epoch is not null && EpochProgress.ObtainMidRun(__instance, epoch, localPlayer))
            MainFile.Logger.Info($"Epoch {epoch.Id} obtained for completing Act {act + 1}");
        return false;
    }

    [HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool ObtainFifteenElitesEpoch(ProgressSaveManager __instance, Player localPlayer)
    {
        if (localPlayer.Character is not IEpochCharacter epochs) return true;
        if (!localPlayer.Character.IsPlayable || epochs.FifteenElitesEpoch is not { } epoch) return false;

        var eliteEncounters = ModelDb.AllEncounters.Where(e => e.RoomType == RoomType.Elite).Select(e => e.Id).ToHashSet();
        if (CountWins(__instance, localPlayer.Character, eliteEncounters) >= 15 && EpochProgress.ObtainMidRun(__instance, epoch, localPlayer))
            MainFile.Logger.Info($"Epoch {epoch.Id} obtained for beating 15 Elites");
        return false;
    }

    [HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool ObtainFifteenBossesEpoch(ProgressSaveManager __instance, Player localPlayer)
    {
        if (localPlayer.Character is not IEpochCharacter epochs) return true;
        if (!localPlayer.Character.IsPlayable || epochs.FifteenBossesEpoch is not { } epoch) return false;

        var bossEncounters = ModelDb.Acts.SelectMany(a => a.AllBossEncounters.Select(e => e.Id)).ToHashSet();
        if (CountWins(__instance, localPlayer.Character, bossEncounters) >= 15 && EpochProgress.ObtainMidRun(__instance, epoch, localPlayer))
            MainFile.Logger.Info($"Epoch {epoch.Id} obtained for beating 15 Bosses");
        return false;
    }

    /// <summary>
    /// Only called after a victory.
    /// </summary>
    [HarmonyPatch(typeof(ProgressSaveManager), "CheckAscensionOneCompleted")]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool ObtainAscensionOneEpoch(ProgressSaveManager __instance, SerializablePlayer serializablePlayer, SerializableRun serializableRun)
    {
        if (ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId!) is not IEpochCharacter epochs) return true;
        if (serializableRun.Ascension != 1 || epochs.AscensionOneEpoch is not { } epoch) return false;

        if (EpochProgress.ObtainPostRun(__instance, epoch, serializablePlayer, serializableRun))
            MainFile.Logger.Info($"Epoch {epoch.Id} obtained for beating Ascension 1");
        return false;
    }

    private static int CountWins(ProgressSaveManager manager, CharacterModel character, HashSet<ModelId> encounters) =>
        manager.Progress.EncounterStats.Values
            .Where(s => encounters.Contains(s.Id))
            .Sum(s => s.FightStats.FirstOrDefault(f => f.Character == character.Id)?.Wins ?? 0);
}
