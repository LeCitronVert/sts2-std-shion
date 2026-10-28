using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

/// <summary>
/// Checks the epoch data once, on the first main menu load, and logs every problem as an error.
/// Catches what would otherwise only show up as a crash or a broken timeline in game.
/// </summary>
public static class EpochValidator
{
    public static void Run()
    {
        var errors = new List<string>();
        try
        {
            CheckContent(errors);
            CheckPlacement(errors);
            CheckObtainable(errors);
            CheckTextAndArt(errors);
        }
        catch (Exception ex)
        {
            errors.Add($"Validation crashed: {ex}");
        }

        foreach (var error in errors) MainFile.Logger.Error($"[Epochs] {error}");
        MainFile.Logger.Info($"[Epochs] Validated {DreamersEpoch.All.Count} epochs: {errors.Count} problem(s)");
    }

    /// <summary>
    /// The unlock text and the unlock screens show exactly 3 items of one kind.
    /// </summary>
    private static void CheckContent(List<string> errors)
    {
        var owners = new Dictionary<ModelId, string>();
        foreach (var epoch in DreamersEpoch.All)
        {
            var kinds = new[] { epoch.Cards.Count, epoch.Relics.Count, epoch.Potions.Count }.Where(n => n > 0).ToList();
            if (kinds.Count > 1) errors.Add($"{epoch.Id} unlocks more than one kind of content");
            if (kinds.Count == 1 && kinds[0] != 3) errors.Add($"{epoch.Id} unlocks {kinds[0]} items, the unlock text needs exactly 3");

            IEnumerable<AbstractModel> content = [.. epoch.Cards, .. epoch.Relics, .. epoch.Potions];
            foreach (var model in content)
            {
                if (!owners.TryAdd(model.Id, epoch.Id))
                    errors.Add($"{model.Id} is unlocked by both {owners[model.Id]} and {epoch.Id}");
            }
        }

        foreach (var character in ModelDb.AllCharacters.Where(c => c is IEpochCharacter))
        {
            IEnumerable<AbstractModel> starters = [.. character.StartingDeck, .. character.StartingRelics];
            foreach (var model in starters.Where(m => owners.ContainsKey(m.Id)).DistinctBy(m => m.Id))
                errors.Add($"{model.Id} is in {character.Id}'s starting kit but locked by {owners[model.Id]}");
        }
    }

    /// <summary>
    /// Two epochs on the same era and row overlap and break the timeline's slot matching.
    /// </summary>
    private static void CheckPlacement(List<string> errors)
    {
        var collisions = EpochModel.AllEpochIds.Select(EpochModel.Get)
            .GroupBy(e => (e.Era, e.EraPosition))
            .Where(g => g.Count() > 1);
        foreach (var group in collisions)
            errors.Add($"{string.Join(", ", group.Select(e => e.Id))} share era {group.Key.Era} row {group.Key.EraPosition}");
    }

    /// <summary>
    /// Every epoch needs a slot on the timeline (another epoch's expansion) and a way to be obtained.
    /// </summary>
    private static void CheckObtainable(List<string> errors)
    {
        var slotted = EpochModel.AllEpochIds.SelectMany(id => EpochModel.Get(id).GetTimelineExpansion()).Select(e => e.Id).ToHashSet();
        var fromCharacters = ModelDb.AllCharacters.OfType<IEpochCharacter>()
            .SelectMany(c => new[] { c.UnlockEpoch, c.Act1Epoch, c.Act2Epoch, c.Act3Epoch, c.FifteenElitesEpoch, c.FifteenBossesEpoch, c.AscensionOneEpoch })
            .OfType<EpochModel>()
            .Select(e => e.Id)
            .ToHashSet();

        foreach (var epoch in DreamersEpoch.All)
        {
            if (!slotted.Contains(epoch.Id))
                errors.Add($"{epoch.Id} is in no epoch's timeline expansion: it never gets a slot");

            var hasCustomCondition = epoch.GetType().GetMethod(nameof(DreamersEpoch.IsEarnedPostRun))!.DeclaringType != typeof(DreamersEpoch);
            if (!fromCharacters.Contains(epoch.Id) && !hasCustomCondition)
                errors.Add($"{epoch.Id} is in no character epoch slot and has no IsEarnedPostRun: it can't be obtained");
        }
    }

    private static void CheckTextAndArt(List<string> errors)
    {
        foreach (var epoch in DreamersEpoch.All)
        {
            List<string> keys = [$"{epoch.Id}.title", $"{epoch.Id}.description", $"{epoch.Id}.unlockInfo"];
            if (epoch.GetType().BaseType is { IsGenericType: true } baseType && baseType.GetGenericTypeDefinition() == typeof(CharacterUnlockEpoch<,>))
                keys.Add($"{epoch.Id}.unlockText");
            if (epoch.StoryId is { } storyId)
                keys.Add($"STORY_{storyId.ToUpperInvariant()}");

            foreach (var key in keys.Where(k => !LocString.Exists("epochs", k)))
                errors.Add($"Missing epochs.json key {key}");

            foreach (var path in new[] { epoch.CustomRealPortraitPath, epoch.CustomPackedPortraitPath }.Where(p => !ResourceLoader.Exists(p)))
                errors.Add($"{epoch.Id} portrait not found: {path}");
        }
    }
}
