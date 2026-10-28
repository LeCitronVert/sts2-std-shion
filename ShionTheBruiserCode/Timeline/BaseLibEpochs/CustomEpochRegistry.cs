using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

/// <summary>
/// In-mod copy of what BaseLib PR #331 (https://github.com/Alchyr/BaseLib-StS2/pull/331) brings, minus custom eras.
/// Everything in this folder mirrors the PR's names so the mod code built on it barely changes.
///
/// Switching to BaseLib once it ships the PR:
///  1. Delete this folder and the <c>CustomEpochRegistry.Register</c> call in MainFile.
///  2. Replace the namespace <c>Timeline.BaseLibEpochs</c> with <c>BaseLib.Abstracts</c> in usings.
///  3. On the characters, drop <c>IEpochCharacter</c> and add <c>override</c> to their epoch properties.
///     Replace the remaining <c>IEpochCharacter</c> references (Timeline folder) with <c>CustomCharacterModel</c>.
///  4. Check BaseLib's version of the PR bugs noted in <see cref="CharacterEpochPatches"/>.
/// </summary>
public static class CustomEpochRegistry
{
    public static readonly List<CustomEpochModel> CustomEpochs = [];
    public static readonly List<CustomStoryModel> CustomStories = [];

    /// <summary>
    /// Registers every concrete CustomStoryModel and CustomEpochModel of the assembly into the game's dictionaries.
    /// Must run during mod init: the multiplayer epoch id cache is built right after.
    /// </summary>
    public static void Register(Assembly assembly)
    {
        var storyTypeDictionary = (Dictionary<string, Type>)AccessTools.Field(typeof(StoryModel), "_storyTypeDictionary").GetValue(null)!;
        foreach (var story in Instantiate<CustomStoryModel>(assembly))
        {
            CustomStories.Add(story);
            // Slugified to match what the game does for lookups
            storyTypeDictionary[StringHelper.Slugify(story.IdInternal)] = story.GetType();
        }

        var epochTypeDictionary = (Dictionary<string, Type>)AccessTools.Field(typeof(EpochModel), "_epochTypeDictionary").GetValue(null)!;
        var typeToIdDictionary = (Dictionary<Type, string>)AccessTools.Field(typeof(EpochModel), "_typeToIdDictionary").GetValue(null)!;
        foreach (var epoch in Instantiate<CustomEpochModel>(assembly))
        {
            CustomEpochs.Add(epoch);
            epochTypeDictionary[epoch.Id] = epoch.GetType();
            typeToIdDictionary[epoch.GetType()] = epoch.Id;
        }

        MainFile.Logger.Info($"Registered {CustomEpochs.Count} custom epochs and {CustomStories.Count} custom stories");
    }

    private static IEnumerable<T> Instantiate<T>(Assembly assembly) =>
        assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(T).IsAssignableFrom(t))
            .Select(t => (T)Activator.CreateInstance(t)!);

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(EpochModel), nameof(EpochModel.AllEpochIds), MethodType.Getter)]
        [HarmonyPostfix]
        private static void InsertEpochIds(ref IReadOnlyList<string> __result)
        {
            __result = [.. __result, .. CustomEpochs.Select(e => e.Id)];
        }

        [HarmonyPatch(typeof(SaveManager), "GetCardUnlockEpochIds")]
        [HarmonyPostfix]
        private static void InsertCardUnlockCustomEpochs(ref string[] __result)
        {
            __result = [.. __result, .. CustomEpochs.Where(e => e.UnlocksCardsInternal).Select(e => e.Id)];
        }
    }
}
