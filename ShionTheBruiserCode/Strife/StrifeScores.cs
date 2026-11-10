using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using FileAccess = Godot.FileAccess;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Damage dealt to the Act 4 Architect: best score per character and mode (solo / multiplayer), and each run's score,
/// in a file of the current profile (local only, like the Chaos Emerald unlocks).
/// Shown in the run history and the Chaos Emeralds menu once the Architect was "defeated" at least once.
/// </summary>
public static class StrifeScores
{
    private const string SaveFileName = "shionthebruiser_strife_scores.json";
    public const string Table = "main_menu_ui";

    private sealed class Data
    {
        public bool Defeated { get; set; }
        /// <summary>"(character id)|solo" or "(character id)|multi" → best damage.</summary>
        public Dictionary<string, long> Best { get; set; } = new();
        /// <summary>Run start time → damage that run.</summary>
        public Dictionary<string, long> Runs { get; set; } = new();
    }

    private static Data? _data;
    private static int? _profileId;
    private static bool _recordedThisRun;

    /// <summary>Damage dealt to the Architect in the current run (set live during the fight).</summary>
    public static long CurrentRunDamage { get; set; }

    private static string SavePath => SaveManager.Instance.GetProfileScopedPath(SaveFileName);

    private static Data Current
    {
        get
        {
            var profile = SaveManager.Instance.CurrentProfileId;
            if (_data == null || _profileId != profile)
            {
                _data = Load();
                _profileId = profile;
            }
            return _data;
        }
    }

    public static bool HasDefeatedArchitect => Current.Defeated;

    public static string Key(ModelId character, bool multiplayer) => $"{character.Entry}|{(multiplayer ? "multi" : "solo")}";

    public static long? Best(ModelId character, bool multiplayer) =>
        Current.Best.TryGetValue(Key(character, multiplayer), out var best) ? best : null;

    public static long? RunDamage(long startTime) => Current.Runs.TryGetValue(startTime.ToString(), out var d) ? d : null;

    /// <summary>All best scores: (character, multiplayer, damage), sorted by character then mode.</summary>
    public static IEnumerable<(CharacterModel Character, bool Multiplayer, long Damage)> AllBest()
    {
        foreach (var (key, damage) in Current.Best.OrderBy(kv => kv.Key))
        {
            var parts = key.Split('|');
            if (parts.Length != 2) continue;
            var character = ModelDb.AllCharacters.FirstOrDefault(c => c.Id.Entry == parts[0]);
            if (character != null) yield return (character, parts[1] == "multi", damage);
        }
    }

    public static void MarkDefeated()
    {
        if (Current.Defeated) return;
        Current.Defeated = true;
        Save();
    }

    public static void ResetForNewRun()
    {
        CurrentRunDamage = 0;
        _recordedThisRun = false;
    }

    /// <summary>Called once when the run ends (won or lost), if the Architect took damage.</summary>
    public static void RecordRun(RunState state, long startTime)
    {
        if (_recordedThisRun || CurrentRunDamage <= 0) return;
        _recordedThisRun = true;
        var me = LocalContext.GetMe(state.Players);
        if (me == null) return;

        var data = Current;
        var key = Key(me.Character.Id, state.Players.Count > 1);
        if (!data.Best.TryGetValue(key, out var best) || CurrentRunDamage > best) data.Best[key] = CurrentRunDamage;
        data.Runs[startTime.ToString()] = CurrentRunDamage;
        Save();
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
            MainFile.Logger.Error($"Could not read Strife score file: {ex}");
            return new Data();
        }
    }

    private static void Save()
    {
        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                MainFile.Logger.Error($"Could not open Strife score file for writing: {FileAccess.GetOpenError()}");
                return;
            }
            file.StoreString(JsonSerializer.Serialize(Current));
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not write Strife score file: {ex}");
        }
    }

    public static string FormatBestLine(CharacterModel character, bool multiplayer, long damage)
    {
        var line = new LocString(Table, "SHIONTHEBRUISER-STRIFE_SCORES.line");
        line.Add("Character", StrifeDialogueEvent.ClassName(character));
        line.Add("Mode", new LocString(Table, multiplayer ? "SHIONTHEBRUISER-STRIFE_SCORES.multiplayer" : "SHIONTHEBRUISER-STRIFE_SCORES.solo"));
        line.Add("Damage", damage);
        return line.GetFormattedText();
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.OnEnded))]
public static class StrifeScoresRunEndPatch
{
    private static readonly AccessTools.FieldRef<RunManager, long> StartTimeRef =
        AccessTools.FieldRefAccess<RunManager, long>("_startTime");

    [HarmonyPrefix]
    public static void Prefix(RunManager __instance, bool isVictory)
    {
        if (StrifeRun.State is not { } state) return;
        StrifeScores.RecordRun(state, StartTimeRef(__instance));
        // (After a true victory the game calls this again with false: the music keeps going then)
        if (!isVictory && !StrifeVictory.IsStrifeWin) StrifeMusic.Stop();
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
public static class StrifeScoresLaunchPatch
{
    [HarmonyPostfix]
    public static void Postfix() => StrifeScores.ResetForNewRun();
}

/// <summary>Run history: the damage dealt to the Act 4 Architect, under the run's quote.</summary>
[HarmonyPatch(typeof(NRunHistory), "LoadDeathQuote")]
public static class StrifeRunHistoryPatch
{
    private static readonly AccessTools.FieldRef<NRunHistory, MegaRichTextLabel> QuoteRef =
        AccessTools.FieldRefAccess<NRunHistory, MegaRichTextLabel>("_deathQuoteLabel");

    [HarmonyPostfix]
    public static void Postfix(NRunHistory __instance, RunHistory history, ModelId characterId)
    {
        if (!StrifeScores.HasDefeatedArchitect || StrifeScores.RunDamage(history.StartTime) is not { } damage) return;
        var label = QuoteRef(__instance);
        if (label == null || !GodotObject.IsInstanceValid(label)) return;

        var line = new LocString(StrifeScores.Table, "SHIONTHEBRUISER-STRIFE_SCORES.run_history");
        line.Add("Damage", damage);
        line.Add("Best", StrifeScores.Best(characterId, history.Players.Count > 1) ?? damage);
        label.Text += "\n" + line.GetFormattedText();
    }
}

/// <summary>The true victory: "Victory!", placeholder text, damage dealt, and confetti.</summary>
[HarmonyPatch(typeof(NGameOverScreen), "InitializeBannerAndQuote")]
public static class StrifeGameOverPatch
{
    private static readonly AccessTools.FieldRef<NGameOverScreen, NCommonBanner> BannerRef =
        AccessTools.FieldRefAccess<NGameOverScreen, NCommonBanner>("_banner");
    private static readonly AccessTools.FieldRef<NGameOverScreen, MegaRichTextLabel> VictoryLabelRef =
        AccessTools.FieldRefAccess<NGameOverScreen, MegaRichTextLabel>("_victoryDamageLabel");

    private const int ConfettiBursts = 4;
    private const float ConfettiDelay = 1.1f;

    [HarmonyPostfix]
    public static void Postfix(NGameOverScreen __instance)
    {
        if (!StrifeVictory.IsStrifeWin || StrifeRun.State is not { } state) return;
        var me = LocalContext.GetMe(state.Players);

        BannerRef(__instance)?.label.SetTextAutoSize(new LocString(StrifeScores.Table, "SHIONTHEBRUISER-STRIFE_VICTORY.banner").GetRawText());

        var text = new LocString(StrifeScores.Table, "SHIONTHEBRUISER-STRIFE_VICTORY.text");
        text.Add("ClassName", me == null ? "" : StrifeDialogueEvent.ClassName(me.Character));
        text.Add("Damage", StrifeScores.CurrentRunDamage);
        if (VictoryLabelRef(__instance) is { } label && GodotObject.IsInstanceValid(label)) label.Text = text.GetFormattedText();

        for (var i = 0; i < ConfettiBursts; i++)
        {
            var timer = __instance.GetTree().CreateTimer(0.3f + i * ConfettiDelay);
            timer.Timeout += () =>
            {
                if (!GodotObject.IsInstanceValid(__instance)) return;
                var confetti = GD.Load<PackedScene>(Path.Join(MainFile.ResPath, "scenes", "fx", "confetti.tscn")).Instantiate<Node2D>();
                confetti.ZIndex = 100;
                __instance.AddChild(confetti);
                confetti.Call("play");
            };
        }
    }
}
