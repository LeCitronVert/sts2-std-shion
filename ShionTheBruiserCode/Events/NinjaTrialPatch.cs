using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Events;

/// <summary>
/// The Trial (vanilla): adds a 4th defendant, the Ninja (1 in 4, the 3 vanilla ones keep equal odds).
/// Innocent: add Doubt, upgrade a card. Guilty: take 8 damage, obtain 2 relics, Kusarigama/Nunchaku/Kunai/Shuriken
/// first (random among the ones you don't have), truly random once you have them.
/// Visuals copy vanilla: the "trial started" portrait with the defendant drawn over it.
/// Text: localization/*/events.json, "TRIAL.pages.NINJA*" keys.
/// </summary>
[HarmonyPatch(typeof(Trial), "Accept")]
public static class NinjaTrialPatch
{
    private const int Damage = 8;
    private const int Relics = 2;

    public static string DefendantPath => "events/ninja_trial.png".ImagePath();
    private static string TrialStartedPath => ImageHelper.GetImagePath("events/trial_started.png");

    private static readonly MethodInfo L10NLookupMethod = AccessTools.Method(typeof(EventModel), "L10NLookup");
    private static readonly MethodInfo SetEventStateMethod = AccessTools.Method(typeof(EventModel), "SetEventState");
    private static readonly MethodInfo SetTrialFinishedMethod = AccessTools.Method(typeof(Trial), "SetTrialFinished");

    private static IEnumerable<RelicModel> NinjaRelics =>
    [
        ModelDb.Relic<Kusarigama>(),
        ModelDb.Relic<Nunchaku>(),
        ModelDb.Relic<Kunai>(),
        ModelDb.Relic<Shuriken>(),
    ];

    [HarmonyPrefix]
    public static bool Prefix(Trial __instance, ref Task __result)
    {
        // Own roll before vanilla's: deterministic (the event's Rng), 1 in 4
        if (__instance.Rng.NextInt(4) != 0) return true;

        Start(__instance);
        __result = Task.CompletedTask;
        return false;
    }

    private static LocString L10NLookup(EventModel trial, string key) =>
        (LocString)L10NLookupMethod.Invoke(trial, [key])!;

    private static void Start(Trial trial)
    {
        var room = NEventRoom.Instance;
        if (LocalContext.IsMe(trial.Owner) && room?.Layout is { } layout)
        {
            layout.RemoveNodesOnPortrait();
            layout.AddVfxAnchoredToPortrait(CreateDefendant());
            room.SetPortrait(PreloadManager.Cache.GetTexture2D(TrialStartedPath));
        }

        EventOption[] options =
        [
            new(trial, () => Innocent(trial), "TRIAL.pages.NINJA.options.INNOCENT", HoverTipFactory.FromCardWithCardHoverTips<Doubt>()),
            new EventOption(trial, () => Guilty(trial), "TRIAL.pages.NINJA.options.GUILTY").ThatDoesDamage(Damage),
        ];

        var description = L10NLookup(trial, "TRIAL.trialFormat");
        description.Add(new StringVar("TrialStory", L10NLookup(trial, "TRIAL.pages.NINJA.description").GetRawText()));
        SetEventStateMethod.Invoke(trial, [description, options]);
    }

    /// <summary>
    /// Same placement as vanilla's trial_merchant_vfx scene: a Node2D at (292, 68) on the portrait, the defendant on the stand.
    /// </summary>
    private static Node2D CreateDefendant()
    {
        var root = new Node2D { Position = new Vector2(292f, 68f), Scale = new Vector2(1.04f, 1.04f) };
        root.AddChild(new Sprite2D
        {
            Texture = PreloadManager.Cache.GetTexture2D(DefendantPath),
            Position = new Vector2(568f, 700f),
            Scale = new Vector2(0.6f, 0.6f),
        });
        return root;
    }

    private static async Task Innocent(Trial trial)
    {
        var owner = trial.Owner!;
        await CardPileCmd.AddCurseToDeck<Doubt>(owner);
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1);
        foreach (var card in await CardSelectCmd.FromDeckForUpgrade(owner, prefs))
        {
            CardCmd.Upgrade(card);
        }
        SetTrialFinishedMethod.Invoke(trial, ["TRIAL.pages.NINJA_INNOCENT.description"]);
    }

    private static async Task Guilty(Trial trial)
    {
        var owner = trial.Owner!;
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), owner.Creature, Damage, ValueProp.Unblockable | ValueProp.Unpowered, null, null);

        var ninjaRelics = NinjaRelics.Where(r => owner.Relics.All(owned => owned.Id != r.Id)).ToList();
        for (var i = 0; i < Relics; i++)
        {
            RelicModel relic;
            if (ninjaRelics.Count > 0)
            {
                relic = trial.Rng.NextItem(ninjaRelics)!;
                ninjaRelics.Remove(relic);
            }
            else
            {
                relic = RelicFactory.PullNextRelicFromFront(owner);
            }
            await RelicCmd.Obtain(relic.ToMutable(), owner);
        }
        SetTrialFinishedMethod.Invoke(trial, ["TRIAL.pages.NINJA_GUILTY.description"]);
    }
}

/// <summary>
/// Preloads the Ninja's picture with the Trial's other assets.
/// </summary>
[HarmonyPatch(typeof(Trial), nameof(Trial.GetAssetPaths))]
public static class NinjaTrialAssetsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<string> __result)
    {
        __result = [..__result, NinjaTrialPatch.DefendantPath];
    }
}
