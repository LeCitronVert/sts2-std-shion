using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Act 4's first node, shown on the map as a shadowed Grey "Chaos Emeralds" relic.
/// Fully heals the player (Ascension doesn't reduce it, unlike other Ancients) and gives Gold. No dialogue, no choice.
/// </summary>
public class StrifeShrine : CustomAncientModel
{
    private static readonly string IconPath = "emeralds/emerald_combo_grey.png".RelicImagePath();
    private static readonly string OutlinePath = "emeralds/emerald_combo_white.png".RelicImagePath();

    /// <summary>The "shadow" tint of the emerald (map icon and scene).</summary>
    public static readonly Color ShadowTint = new(0.13f, 0.12f, 0.17f);

    protected override OptionPools MakeOptionPools => new(MakePool(Array.Empty<RelicModel>()));

    public override bool IsValidForAct(ActModel act) => StrifeAct.IsStrife(act);

    public override string? CustomScenePath => Path.Join(MainFile.ResPath, "scenes", "ancients", "strife_shrine.tscn");
    public override string? CustomMapIconPath => IconPath;
    public override string? CustomMapIconOutlinePath => OutlinePath;
    public override string? CustomRunHistoryIconPath => IconPath;
    public override string? CustomRunHistoryIconOutlinePath => OutlinePath;

    public override Color DialogueColor => new("2b2838");
    public override Color ButtonColor => new(0.08f, 0.07f, 0.1f, 0.6f);

    protected override AncientDialogueSet DefineDialogues() => new()
    {
        FirstVisitEverDialogue = null,
        CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>(),
        AgnosticDialogues = [],
    };

    // No relic to pick: the event goes straight to its DONE page
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => [];

    protected override async Task BeforeEventStarted(bool isPreFinished)
    {
        // Not base: other Ancients heal less with Weary Traveler (Ascension), this one always heals to full
        if (isPreFinished) return;
        var creature = Owner!.Creature;
        await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp, playAnim: false);
        await PlayerCmd.GainGold(StrifeConfig.ShrineGold, Owner);
    }
}

/// <summary>The shrine's map node is the Grey Chaos Emeralds icon, darkened into a silhouette. Visual only.</summary>
[HarmonyPatch(typeof(NAncientMapPoint), nameof(NAncientMapPoint._Ready))]
public static class StrifeShrineMapIconPatch
{
    private static readonly AccessTools.FieldRef<NAncientMapPoint, TextureRect> IconRef =
        AccessTools.FieldRefAccess<NAncientMapPoint, TextureRect>("_icon");

    [HarmonyPostfix]
    public static void Postfix(NAncientMapPoint __instance)
    {
        var runState = MegaCrit.Sts2.Core.Runs.RunManager.Instance.DebugOnlyGetState();
        if (runState == null || !StrifeAct.IsStrife(runState)) return;
        var icon = IconRef(__instance);
        if (icon != null && GodotObject.IsInstanceValid(icon)) icon.SelfModulate = StrifeShrine.ShadowTint;
    }
}
