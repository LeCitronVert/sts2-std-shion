using System.Runtime.CompilerServices;
using System.Text.Json;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using FileAccess = Godot.FileAccess;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// Finds the partners a party dialogue group needs among the other players' characters.
/// Returns them in the order the group's speaker indices refer to, or null when the party doesn't match.
/// </summary>
public delegate IReadOnlyList<CharacterModel>? PartyMatcher(CharacterModel self, IReadOnlyList<CharacterModel> partners);

/// <summary>
/// One Neow conversation between the local character and party members.
/// Each speaker is <see cref="Self"/> or an index into the partners matched by the group.
/// Text lives in the "ancients" table under NEOW.party.{self}.{group}.{dialogue}-{line}, with an optional ".next" key
/// for the button (falls back to NEOW.party.next).
/// </summary>
public sealed class PartyDialogue(bool repeating, params int[] speakers)
{
    public const int Self = -1;

    public bool IsRepeating => repeating;
    public IReadOnlyList<int> Speakers => speakers;
}

public sealed class PartyDialogueGroup(string key, PartyMatcher match, params PartyDialogue[] dialogues)
{
    public string Key => key;
    public PartyMatcher Match => match;
    public IReadOnlyList<PartyDialogue> Dialogues => dialogues;
}

/// <summary>
/// Cross-character Neow dialogues, multiplayer only. Picked on each client with the chaotic RNG, like vanilla
/// dialogue, so nothing here touches shared state.
/// </summary>
public static class PartyDialogues
{
    private const string LocTable = "ancients";
    private const string FallbackNextKey = "NEOW.party.next";

    // Chance to replace the vanilla dialogue when a party dialogue is available.
    private const float PartyChance = 0.4f; //todo : choose weight
    private const float PartyChanceOverVisitSpecific = 0.25f; //todo : choose weight

    private static readonly Lazy<Dictionary<string, IReadOnlyList<PartyDialogueGroup>>> Groups = new(() => new()
    {
        [ModelDb.Character<Character.ShionTheBruiser>().Id.Entry] = ShionPartyDialogues.Groups,
    });

    private static readonly ConditionalWeakTable<AncientDialogueLine, CharacterModel> PartnerSpeakers = new();

    /// <summary>The partner speaking this line, if it isn't the local character.</summary>
    public static bool TryGetPartnerSpeaker(AncientDialogueLine line, out CharacterModel character) =>
        PartnerSpeakers.TryGetValue(line, out character!);

    /// <summary>
    /// Rolls whether a party dialogue replaces the vanilla one and builds its lines.
    /// <paramref name="vanillaIsVisitSpecific"/> lowers the chance so first-visit / Nth-visit dialogue mostly wins.
    /// </summary>
    public static IReadOnlyList<AncientDialogueLine>? TryPick(Player owner, bool vanillaIsVisitSpecific)
    {
        var self = owner.Character;
        if (!Groups.Value.TryGetValue(self.Id.Entry, out var groups)) return null;

        var partners = owner.RunState.Players.Where(p => p != owner).Select(p => p.Character).ToList();
        var candidates = new List<(PartyDialogueGroup Group, int Index, IReadOnlyList<CharacterModel> Matched)>();
        foreach (var group in groups)
        {
            var matched = group.Match(self, partners);
            if (matched == null) continue;
            foreach (var index in EligibleIndices(self, group))
            {
                candidates.Add((group, index, matched));
            }
        }
        if (candidates.Count == 0) return null;

        var chance = vanillaIsVisitSpecific ? PartyChanceOverVisitSpecific : PartyChance;
        if (Rng.Chaotic.NextFloat() >= chance) return null;

        var (pickedGroup, pickedIndex, pickedPartners) = candidates[Rng.Chaotic.NextInt(candidates.Count)];
        if (!pickedGroup.Dialogues[pickedIndex].IsRepeating) MarkSeen(SeenKey(self, pickedGroup, pickedIndex));
        return BuildLines(self, pickedGroup, pickedIndex, pickedPartners);
    }

    /// <summary>
    /// Dialogues unlock in order: non-repeating ones are shown once each, and a dialogue only becomes available once
    /// every non-repeating dialogue before it has been seen.
    /// </summary>
    private static IEnumerable<int> EligibleIndices(CharacterModel self, PartyDialogueGroup group)
    {
        for (var i = 0; i < group.Dialogues.Count; i++)
        {
            if (group.Dialogues[i].IsRepeating)
            {
                yield return i;
                continue;
            }
            if (Seen.Contains(SeenKey(self, group, i))) continue;
            yield return i;
            yield break;
        }
    }

    private static IReadOnlyList<AncientDialogueLine> BuildLines(CharacterModel self, PartyDialogueGroup group, int index,
        IReadOnlyList<CharacterModel> partners)
    {
        var speakers = group.Dialogues[index].Speakers;
        var lines = new List<AncientDialogueLine>(speakers.Count);
        for (var i = 0; i < speakers.Count; i++)
        {
            var key = $"NEOW.party.{self.Id.Entry}.{group.Key}.{index}-{i}";
            var line = new AncientDialogueLine("")
            {
                LineText = new LocString(LocTable, key),
                Speaker = AncientDialogueSpeaker.Character,
            };
            if (i < speakers.Count - 1)
            {
                line.NextButtonText = new LocString(LocTable, LocString.Exists(LocTable, key + ".next") ? key + ".next" : FallbackNextKey);
            }
            if (speakers[i] != PartyDialogue.Self)
            {
                PartnerSpeakers.AddOrUpdate(line, partners[speakers[i]]);
            }
            lines.Add(line);
        }
        return lines;
    }


    // ── Matchers ─────────────────────────────────────────────────────────────

    /// <summary>Every listed character is in the party (each matched to a different player).</summary>
    public static PartyMatcher All(params Func<CharacterModel>[] required) => (_, partners) =>
    {
        var pool = partners.ToList();
        var matched = new List<CharacterModel>();
        foreach (var get in required)
        {
            var id = get().Id;
            var found = pool.FirstOrDefault(p => p.Id == id);
            if (found == null) return null;
            pool.Remove(found);
            matched.Add(found);
        }
        return matched;
    };

    /// <summary>At least one of the listed characters is in the party.</summary>
    public static PartyMatcher AnyOf(params Func<CharacterModel>[] options) => (_, partners) =>
    {
        var ids = options.Select(get => get().Id).ToList();
        var found = partners.FirstOrDefault(p => ids.Contains(p.Id));
        return found == null ? null : [found];
    };

    /// <summary>Another player picked the same character.</summary>
    public static PartyMatcher SameCharacter() => (self, partners) =>
    {
        var found = partners.FirstOrDefault(p => p.Id == self.Id);
        return found == null ? null : [found];
    };

    /// <summary>A character from neither the base game nor this mod.</summary>
    public static PartyMatcher OtherModded() => (_, partners) =>
    {
        var found = partners.FirstOrDefault(p =>
            p.GetType().Assembly != typeof(CharacterModel).Assembly && p.GetType().Assembly != typeof(PartyDialogues).Assembly);
        return found == null ? null : [found];
    };


    // ── Seen dialogues (per profile, local machine only) ─────────────────────

    private const string SaveFileName = "shionthebruiser_party_dialogues.json";

    private static HashSet<string>? _seen;
    private static int? _seenProfileId;

    private static string SavePath => SaveManager.Instance.GetProfileScopedPath(SaveFileName);

    private static string SeenKey(CharacterModel self, PartyDialogueGroup group, int index) => $"{self.Id.Entry}.{group.Key}.{index}";

    private static HashSet<string> Seen
    {
        get
        {
            var profileId = SaveManager.Instance.CurrentProfileId;
            if (_seen == null || _seenProfileId != profileId)
            {
                _seen = LoadSeen();
                _seenProfileId = profileId;
            }
            return _seen;
        }
    }

    private static void MarkSeen(string key)
    {
        if (Seen.Add(key)) SaveSeen(Seen);
    }

    private static HashSet<string> LoadSeen()
    {
        try
        {
            if (!FileAccess.FileExists(SavePath)) return [];
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            return JsonSerializer.Deserialize<HashSet<string>>(file.GetAsText()) ?? [];
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not read party dialogue save file: {ex}");
            return [];
        }
    }

    private static void SaveSeen(HashSet<string> seen)
    {
        try
        {
            var json = JsonSerializer.Serialize(seen.Order().ToList());
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                MainFile.Logger.Error($"Could not open party dialogue save file for writing: {FileAccess.GetOpenError()}");
                return;
            }
            file.StoreString(json);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not write party dialogue save file: {ex}");
        }
    }
}
