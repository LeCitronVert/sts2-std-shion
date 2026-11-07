using System.Text.Json;
using MegaCrit.Sts2.Core.Saves;
using FileAccess = Godot.FileAccess;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// Cosmo's data in the local profile (never shared, never in the run save):
///  - which characters ended their last run holding a Chaos Emerald (solo: Cosmo greets them at the next start),
///  - whether the next eligible multiplayer run should start with Cosmo (she alternates with Neow, the host decides),
///  - which conditional dialogues were already shown (they play once each).
/// </summary>
public static class CosmoSaveData
{
    private const string SaveFileName = "shionthebruiser_cosmo.json";

    private sealed class Data
    {
        public HashSet<string> HadEmeraldLastRun { get; set; } = [];
        public bool MultiplayerCosmoNext { get; set; } = true;
        public HashSet<string> SeenDialogues { get; set; } = [];
    }

    private static Data? _data;
    private static int? _profileId;

    private static string SavePath => SaveManager.Instance.GetProfileScopedPath(SaveFileName);

    private static Data Current
    {
        get
        {
            var profileId = SaveManager.Instance.CurrentProfileId;
            if (_data == null || _profileId != profileId)
            {
                _data = Load();
                _profileId = profileId;
            }
            return _data;
        }
    }

    public static bool HadEmeraldLastRun(string characterId) => Current.HadEmeraldLastRun.Contains(characterId);

    public static void SetHadEmeraldLastRun(string characterId, bool hadEmerald)
    {
        var changed = hadEmerald ? Current.HadEmeraldLastRun.Add(characterId) : Current.HadEmeraldLastRun.Remove(characterId);
        if (changed) Save();
    }

    public static bool MultiplayerCosmoNext => Current.MultiplayerCosmoNext;

    public static void SetMultiplayerCosmoNext(bool value)
    {
        if (Current.MultiplayerCosmoNext == value) return;
        Current.MultiplayerCosmoNext = value;
        Save();
    }

    public static bool WasSeen(string dialogueKey) => Current.SeenDialogues.Contains(dialogueKey);

    public static void MarkSeen(string dialogueKey)
    {
        if (Current.SeenDialogues.Add(dialogueKey)) Save();
    }

    private static Data Load()
    {
        try
        {
            if (!FileAccess.FileExists(SavePath)) return new Data();
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            return JsonSerializer.Deserialize<Data>(file.GetAsText()) ?? new Data();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not read Cosmo save file: {ex}");
            return new Data();
        }
    }

    private static void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                MainFile.Logger.Error($"Could not open Cosmo save file for writing: {FileAccess.GetOpenError()}");
                return;
            }
            file.StoreString(json);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not write Cosmo save file: {ex}");
        }
    }
}
