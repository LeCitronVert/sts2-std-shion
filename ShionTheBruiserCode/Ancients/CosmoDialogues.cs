using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>When a conditional Cosmo dialogue can play (the interaction sheets' extra column).</summary>
public enum CosmoDialogueCondition
{
    None,
    /// <summary>"Only if playing Solo".</summary>
    Solo,
    /// <summary>Solo, and the character held a Chaos Emerald in their last run (the emerald "found last time").</summary>
    SoloHadEmerald,
    /// <summary>"Only if playing MP without other STD characters in the party".</summary>
    MultiplayerWithoutOtherDreamers,
    /// <summary>"Only if playing MP with [every other Dreamer] in the party".</summary>
    MultiplayerWithAllOtherDreamers,
    /// <summary>"Only if playing MP, with a character from another mod (non-STD nor vanilla) in the party".</summary>
    MultiplayerWithOtherModded,
}

/// <summary>
/// Picks Cosmo's dialogue. Her lines live in ancients.json in the base game's format, loaded by BaseLib
/// (dialogue index 0/1/2/3… = visit 0/1/4/7…, "r" = repeatable). On top of that, some dialogues have a condition:
///  - a conditional one-time dialogue ignores its visit slot: it plays on the first visit where its condition holds,
///    once the unconditional one-time dialogues before it have played. It plays once (local profile).
///  - a conditional repeatable dialogue joins the random pool only while its condition holds.
/// Purely cosmetic and local (chaotic RNG, local profile), like vanilla dialogue.
/// </summary>
public static class CosmoDialogues
{
    private const string AnyKey = "ANY";

    // Dialogue index = the sheet's dialogue number - 1
    private static readonly Lazy<Dictionary<(string Character, int Index), CosmoDialogueCondition>> Conditions = new(() => new()
    {
        [(AnyKey, 3)] = CosmoDialogueCondition.SoloHadEmerald,

        [(CharKey<Character.ShionTheBruiser>(), 6)] = CosmoDialogueCondition.Solo,
        [(CharKey<Character.ShionTheBruiser>(), 7)] = CosmoDialogueCondition.MultiplayerWithoutOtherDreamers,
        [(CharKey<Character.ShionTheBruiser>(), 8)] = CosmoDialogueCondition.MultiplayerWithAllOtherDreamers,
        [(CharKey<Character.ShionTheBruiser>(), 9)] = CosmoDialogueCondition.MultiplayerWithAllOtherDreamers,
        [(CharKey<Character.ShionTheBruiser>(), 10)] = CosmoDialogueCondition.MultiplayerWithOtherModded,

        [(CharKey<SeelsTheReckless>(), 9)] = CosmoDialogueCondition.Solo,
        [(CharKey<SeelsTheReckless>(), 10)] = CosmoDialogueCondition.MultiplayerWithoutOtherDreamers,
        [(CharKey<SeelsTheReckless>(), 11)] = CosmoDialogueCondition.MultiplayerWithAllOtherDreamers,
        [(CharKey<SeelsTheReckless>(), 12)] = CosmoDialogueCondition.MultiplayerWithAllOtherDreamers,
        [(CharKey<SeelsTheReckless>(), 13)] = CosmoDialogueCondition.MultiplayerWithOtherModded,
    });

    private static string CharKey<T>() where T : CharacterModel => ModelDb.Character<T>().Id.Entry;

    private static IReadOnlyList<CharacterModel> Dreamers =>
    [
        ModelDb.Character<Character.ShionTheBruiser>(),
        ModelDb.Character<SlalTheEndless>(),
        ModelDb.Character<SeelsTheReckless>(),
        ModelDb.Character<FryTheCommander>(),
    ];

    private static CosmoDialogueCondition ConditionOf(string character, int index) =>
        Conditions.Value.GetValueOrDefault((character, index), CosmoDialogueCondition.None);

    public static bool Holds(CosmoDialogueCondition condition, Player owner)
    {
        var self = owner.Character;
        var partners = owner.RunState.Players.Where(p => p != owner).Select(p => p.Character).ToList();
        var multiplayer = partners.Count > 0;
        bool IsDreamer(CharacterModel c) => Dreamers.Any(d => d.Id == c.Id);

        return condition switch
        {
            CosmoDialogueCondition.None => true,
            CosmoDialogueCondition.Solo => !multiplayer,
            CosmoDialogueCondition.SoloHadEmerald => !multiplayer && CosmoSaveData.HadEmeraldLastRun(self.Id.Entry),
            CosmoDialogueCondition.MultiplayerWithoutOtherDreamers => multiplayer && !partners.Any(IsDreamer),
            CosmoDialogueCondition.MultiplayerWithAllOtherDreamers =>
                multiplayer && Dreamers.Where(d => d.Id != self.Id).All(d => partners.Any(p => p.Id == d.Id)),
            CosmoDialogueCondition.MultiplayerWithOtherModded => multiplayer && partners.Any(p =>
                p.GetType().Assembly != typeof(CharacterModel).Assembly && !IsDreamer(p)),
            _ => false,
        };
    }

    private static string SeenKey(string character, int index) => $"{character}.{index}";

    public static IReadOnlyList<AncientDialogueLine>? Pick(Cosmo cosmo, Player owner)
    {
        var set = cosmo.DialogueSet;
        var self = owner.Character.Id;
        var stats = SaveManager.Instance.Progress.GetStatsForAncient(cosmo.Id);
        var charVisits = stats?.GetVisitsAs(self) ?? 0;
        var totalVisits = stats?.TotalVisits ?? 0;

        AncientDialogue? picked = null;

        if (totalVisits == 0 && set.FirstVisitEverDialogue != null)
        {
            picked = set.FirstVisitEverDialogue;
        }
        else
        {
            var own = set.CharacterDialogues.GetValueOrDefault(self.Entry) ?? [];
            var oneTime = OneTime(own, self.Entry, owner, charVisits) ?? OneTime(set.AgnosticDialogues, AnyKey, owner, charVisits);
            if (oneTime is { } found)
            {
                picked = found.Dialogue;
                if (found.SeenKey != null) CosmoSaveData.MarkSeen(found.SeenKey);
            }
            else
            {
                List<AncientDialogue> pool = [.. RepeatPool(own, self.Entry, owner, charVisits), .. RepeatPool(set.AgnosticDialogues, AnyKey, owner, charVisits)];
                if (pool.Count > 0) picked = pool[Rng.Chaotic.NextInt(pool.Count)];
            }
        }

        if (picked == null) return null;

        var className = owner.Character.Title.GetFormattedText();
        foreach (var line in picked.Lines)
        {
            line.LineText?.Add("Class", className);
        }
        return picked.Lines;
    }

    /// <summary>
    /// The one-time dialogue for this visit: the unconditional one whose slot is this visit,
    /// else the first conditional one that is now unlocked and whose condition holds.
    /// </summary>
    private static (AncientDialogue Dialogue, string? SeenKey)? OneTime(IReadOnlyList<AncientDialogue> dialogues, string character,
        Player owner, int charVisits)
    {
        (AncientDialogue, string?)? conditional = null;
        var earlierPlayed = true;
        for (var i = 0; i < dialogues.Count; i++)
        {
            var dialogue = dialogues[i];
            if (dialogue.IsRepeating) continue;

            var condition = ConditionOf(character, i);
            if (condition == CosmoDialogueCondition.None)
            {
                if (dialogue.VisitIndex == charVisits) return (dialogue, null);
                if (dialogue.VisitIndex > charVisits) earlierPlayed = false;
                continue;
            }

            var key = SeenKey(character, i);
            if (conditional == null && earlierPlayed && !CosmoSaveData.WasSeen(key) && Holds(condition, owner))
                conditional = (dialogue, key);
        }

        return conditional;
    }

    private static IEnumerable<AncientDialogue> RepeatPool(IReadOnlyList<AncientDialogue> dialogues, string character, Player owner, int charVisits)
    {
        for (var i = 0; i < dialogues.Count; i++)
        {
            var dialogue = dialogues[i];
            if (!dialogue.IsRepeating) continue;

            var condition = ConditionOf(character, i);
            if (condition == CosmoDialogueCondition.None)
            {
                if (!(charVisits < dialogue.VisitIndex)) yield return dialogue;
            }
            else if (Holds(condition, owner))
            {
                yield return dialogue;
            }
        }
    }
}

/// <summary>Cosmo's dialogue is picked by <see cref="CosmoDialogues"/> instead of the base game's visit rules.</summary>
[HarmonyPatch(typeof(NAncientEventLayout), nameof(NAncientEventLayout.SetDialogue))]
public static class CosmoDialogueSelectPatch
{
    [HarmonyPrefix]
    public static void Prefix(NAncientEventLayout __instance, ref IReadOnlyList<AncientDialogueLine> lines)
    {
        if (CosmoAccess.AncientEvent(__instance) is not Cosmo cosmo || cosmo.Owner is not { } owner) return;
        try
        {
            if (CosmoDialogues.Pick(cosmo, owner) is { } picked) lines = picked;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not pick Cosmo's dialogue: {ex}");
        }
    }
}
