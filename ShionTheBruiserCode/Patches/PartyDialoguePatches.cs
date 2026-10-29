using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;
using ShionTheBruiser.ShionTheBruiserCode.Ancients;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Multiplayer Neow: may swap the vanilla dialogue for a party dialogue (see <see cref="PartyDialogues"/>).
/// </summary>
[HarmonyPatch(typeof(NAncientEventLayout), nameof(NAncientEventLayout.SetDialogue))]
public static class PartyDialogueSelectPatch
{
    private static readonly AccessTools.FieldRef<NAncientEventLayout, AncientEventModel> AncientEventRef =
        AccessTools.FieldRefAccess<NAncientEventLayout, AncientEventModel>("_ancientEvent");

    [HarmonyPrefix]
    public static void Prefix(NAncientEventLayout __instance, ref IReadOnlyList<AncientDialogueLine> lines)
    {
        if (AncientEventRef(__instance) is not Neow neow || neow.Owner == null) return;
        if (!RunManager.Instance.NetService.Type.IsMultiplayer()) return;

        var vanilla = lines;
        var vanillaIsVisitSpecific = neow.DialogueSet.GetAllDialogues()
            .FirstOrDefault(d => ReferenceEquals(d.Lines, vanilla)) is { IsRepeating: false };
        var party = PartyDialogues.TryPick(neow.Owner, vanillaIsVisitSpecific);
        if (party != null) lines = party;
    }
}

/// <summary>Partner lines are drawn with the partner's icon and bubble colour.</summary>
[HarmonyPatch(typeof(NAncientDialogueLine), nameof(NAncientDialogueLine.Create))]
public static class PartyDialogueSpeakerPatch
{
    [HarmonyPrefix]
    public static void Prefix(AncientDialogueLine line, ref CharacterModel character)
    {
        if (PartyDialogues.TryGetPartnerSpeaker(line, out var partner)) character = partner;
    }
}

/// <summary>Partner lines sit on the left (the ancient's side), mirroring SetAncientAsSpeaker.</summary>
[HarmonyPatch(typeof(NAncientDialogueLine), "SetCharacterAsSpeaker")]
public static class PartyDialogueLeftSidePatch
{
    private static readonly AccessTools.FieldRef<NAncientDialogueLine, AncientDialogueLine> LineRef =
        AccessTools.FieldRefAccess<NAncientDialogueLine, AncientDialogueLine>("_line");

    private static readonly AccessTools.FieldRef<NAncientDialogueLine, Control> IconNodeRef =
        AccessTools.FieldRefAccess<NAncientDialogueLine, Control>("_iconNode");

    [HarmonyPrefix]
    public static bool Prefix(NAncientDialogueLine __instance)
    {
        if (!PartyDialogues.TryGetPartnerSpeaker(LineRef(__instance), out var character)) return true;

        var icon = __instance.GetNode<Control>("%AncientIcon");
        icon.GetNode<TextureRect>("Icon").Texture = character.IconTexture;
        icon.GetNode<TextureRect>("Icon/Outline").Texture = character.IconOutlineTexture;
        IconNodeRef(__instance) = icon;
        var tail = __instance.GetNode<Control>("%DialogueTailLeft");
        tail.Visible = true;
        __instance.GetNode<MarginContainer>("%TextContainer").AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginLeft, 48);
        __instance.GetNode<Control>("%Bubble").SelfModulate = character.DialogueColor;
        tail.SelfModulate = character.DialogueColor;
        return false;
    }
}
