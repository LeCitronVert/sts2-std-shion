using System.Text.Json;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using FileAccess = Godot.FileAccess;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Shared logic for the Chaos Emerald sidequest:
/// Seek (rest site) → Special Stage (Quest card) → Elite challenge → Chaos Emerald relic → win the run to unlock it.
/// </summary>
public static class ChaosEmeraldHelper
{
    public static readonly ChaosEmeraldColor[] AllColors = Enum.GetValues<ChaosEmeraldColor>();

    /// <summary>
    /// Whether the player already used Seek this run (only once per run). Saved with the run.
    /// </summary>
    public static readonly SavedSpireField<Player, bool> HasSought = new(() => false, "ChaosEmeraldSought");

    // ── Character ↔ Emerald ──────────────────────────────────────────────────

    public static ChaosEmeraldColor ColorFor(CharacterModel? character) => character switch
    {
        Character.ShionTheBruiser => ChaosEmeraldColor.Purple,
        SlalTheEndless => ChaosEmeraldColor.Cyan,
        Silent => ChaosEmeraldColor.Green,
        Regent => ChaosEmeraldColor.Yellow,
        Defect or FryTheCommander => ChaosEmeraldColor.Blue,
        Ironclad or SeelsTheReckless => ChaosEmeraldColor.Red,
        _ => ChaosEmeraldColor.Grey,
    };

    public static ChaosEmeraldColor ColorFor(Player player) => ColorFor(player.Character);

    public static string ColorKey(ChaosEmeraldColor color) => color.ToString().ToLowerInvariant();

    public static RelicModel SoloRelic(ChaosEmeraldColor color) => color switch
    {
        ChaosEmeraldColor.Purple => ModelDb.Relic<PurpleChaosEmerald>(),
        ChaosEmeraldColor.Cyan => ModelDb.Relic<CyanChaosEmerald>(),
        ChaosEmeraldColor.Green => ModelDb.Relic<GreenChaosEmerald>(),
        ChaosEmeraldColor.Yellow => ModelDb.Relic<YellowChaosEmerald>(),
        ChaosEmeraldColor.Blue => ModelDb.Relic<BlueChaosEmerald>(),
        ChaosEmeraldColor.Red => ModelDb.Relic<RedChaosEmerald>(),
        _ => ModelDb.Relic<GreyChaosEmerald>(),
    };

    public static RelicModel ComboRelic(ChaosEmeraldColor color) => color switch
    {
        ChaosEmeraldColor.Purple => ModelDb.Relic<PurpleChaosEmeralds>(),
        ChaosEmeraldColor.Cyan => ModelDb.Relic<CyanChaosEmeralds>(),
        ChaosEmeraldColor.Green => ModelDb.Relic<GreenChaosEmeralds>(),
        ChaosEmeraldColor.Yellow => ModelDb.Relic<YellowChaosEmeralds>(),
        ChaosEmeraldColor.Blue => ModelDb.Relic<BlueChaosEmeralds>(),
        ChaosEmeraldColor.Red => ModelDb.Relic<RedChaosEmeralds>(),
        _ => ModelDb.Relic<GreyChaosEmeralds>(),
    };

    /// <summary>
    /// True if the player holds the Emerald (solo or combo) of their own character.
    /// </summary>
    public static bool HasOwnEmerald(Player player)
    {
        var color = ColorFor(player);
        return player.Relics.Any(r => r is ChaosEmeraldRelic emerald && emerald.Color == color);
    }

    public static bool CanSeek(Player player) => !HasSought.Get(player) && !HasOwnEmerald(player);

    // ── Elite challenges ─────────────────────────────────────────────────────

    /// <summary>
    /// Applies one stack of the Special Stage challenge of that color. Challenges affect the whole team.
    /// </summary>
    public static async Task ApplyChallenge(ChaosEmeraldColor color, ICombatState combatState, Player source,
        PlayerChoiceContext choiceContext)
    {
        var players = combatState.Players.Select(p => p.Creature).Where(c => c.IsAlive).ToList();
        switch (color)
        {
            case ChaosEmeraldColor.Purple:
                foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive).ToList())
                {
                    await PowerCmd.Apply<PurpleEmeraldChallenge>(choiceContext, enemy, 1, null, null);
                }
                break;
            case ChaosEmeraldColor.Cyan:
                await PowerCmd.Apply<CyanEmeraldChallenge>(choiceContext, players, 1, null, null);
                break;
            case ChaosEmeraldColor.Green:
                await PowerCmd.Apply<GreenEmeraldChallenge>(choiceContext, players, 1, null, null);
                break;
            case ChaosEmeraldColor.Yellow:
                await PowerCmd.Apply<YellowEmeraldChallenge>(choiceContext, players, 1, null, null);
                break;
            case ChaosEmeraldColor.Blue:
                await PowerCmd.Apply<BlueEmeraldChallenge>(choiceContext, players, 1, null, null);
                break;
            case ChaosEmeraldColor.Red:
                foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive).ToList())
                {
                    await PowerCmd.Apply<RitualPower>(choiceContext, enemy, 1, enemy, null);
                }
                break;
            default:
                await PowerCmd.Apply<GreyEmeraldChallenge>(choiceContext, players, 1, null, null);
                break;
        }
    }


    // ── Save file (per profile, local machine only) ──────────────────────────

    private const string SaveFileName = "shionthebruiser_chaos_emeralds.json";

    private static HashSet<ChaosEmeraldColor>? _unlocked;
    private static int? _unlockedProfileId;

    private static string SavePath => SaveManager.Instance.GetProfileScopedPath(SaveFileName);

    public static IReadOnlySet<ChaosEmeraldColor> UnlockedColors
    {
        get
        {
            var profileId = SaveManager.Instance.CurrentProfileId;
            if (_unlocked == null || _unlockedProfileId != profileId)
            {
                _unlocked = LoadUnlocked();
                _unlockedProfileId = profileId;
            }
            return _unlocked;
        }
    }

    public static bool IsUnlocked(ChaosEmeraldColor color) => UnlockedColors.Contains(color);

    public static bool AllUnlocked() => AllColors.All(IsUnlocked);

    public static void Unlock(IEnumerable<ChaosEmeraldColor> colors)
    {
        var unlocked = (HashSet<ChaosEmeraldColor>)UnlockedColors;
        var changed = false;
        foreach (var color in colors)
        {
            changed |= unlocked.Add(color);
        }
        if (changed) SaveUnlocked(unlocked);
    }

    private static HashSet<ChaosEmeraldColor> LoadUnlocked()
    {
        var result = new HashSet<ChaosEmeraldColor>();
        try
        {
            if (!FileAccess.FileExists(SavePath)) return result;
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            var names = JsonSerializer.Deserialize<List<string>>(file.GetAsText()) ?? [];
            foreach (var name in names)
            {
                if (Enum.TryParse<ChaosEmeraldColor>(name, out var color)) result.Add(color);
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not read Chaos Emerald save file: {ex}");
        }
        return result;
    }

    private static void SaveUnlocked(HashSet<ChaosEmeraldColor> unlocked)
    {
        try
        {
            var json = JsonSerializer.Serialize(AllColors.Where(unlocked.Contains).Select(c => c.ToString()).ToList());
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                MainFile.Logger.Error($"Could not open Chaos Emerald save file for writing: {FileAccess.GetOpenError()}");
                return;
            }
            file.StoreString(json);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not write Chaos Emerald save file: {ex}");
        }
    }
}
