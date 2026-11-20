using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Cards on Cooldown (Fry's Cooldown cards, and Seels' Blood Bend cards under Our Mages Are Different) glow red in the
/// Hand and have their art in black and white until the Cooldown ends. Purely visual: reads the card's state only.
/// </summary>
public static class CooldownLook
{
    public static bool IsOnCooldown(CardModel card) =>
        card is FryCooldownCard { IsOnCooldown: true } || OurMagesCooldown.IsOnCooldown(card);
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.ShouldGlowRed), MethodType.Getter)]
public static class CooldownGlowRedPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!__result && CooldownLook.IsOnCooldown(__instance)) __result = true;
    }
}

/// <summary>UpdatePortrait runs at the end of both Reload and UpdateVisuals, after the game has set the portrait's material.</summary>
[HarmonyPatch(typeof(NCard), "UpdatePortrait")]
public static class CooldownGrayscalePortraitPatch
{
    private static readonly AccessTools.FieldRef<NCard, TextureRect> PortraitRef =
        AccessTools.FieldRefAccess<NCard, TextureRect>("_portrait");

    private static readonly AccessTools.FieldRef<NCard, TextureRect> AncientPortraitRef =
        AccessTools.FieldRefAccess<NCard, TextureRect>("_ancientPortrait");

    [HarmonyPostfix]
    public static void Postfix(NCard __instance)
    {
        // Hidden/locked cards use the game's blur material: leave it alone.
        if (__instance.Visibility != ModelVisibility.Visible) return;

        var gray = __instance.Model is { } model && CooldownLook.IsOnCooldown(model);
        Apply(PortraitRef(__instance), gray);
        Apply(AncientPortraitRef(__instance), gray);
    }

    private static void Apply(TextureRect? portrait, bool gray)
    {
        if (portrait == null || !GodotObject.IsInstanceValid(portrait)) return;
        if (gray) portrait.Material = CreatureLook.Grayscale;
        else if (portrait.Material == CreatureLook.Grayscale) portrait.Material = null;
    }
}
