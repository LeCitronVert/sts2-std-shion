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

    /// <summary>Turns of Cooldown left (0 = not on Cooldown).</summary>
    public static int TurnsLeft(CardModel card) =>
        Math.Max(card is FryCooldownCard fry ? fry.CooldownTurnsLeft : 0, OurMagesCooldown.TurnsLeftOf(card));
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
        CooldownClock.Refresh(__instance);
    }

    private static void Apply(TextureRect? portrait, bool gray)
    {
        if (portrait == null || !GodotObject.IsInstanceValid(portrait)) return;
        if (gray) portrait.Material = CreatureLook.Grayscale;
        else if (portrait.Material == CreatureLook.Grayscale) portrait.Material = null;
    }
}

/// <summary>
/// Shows a small clock (scenes/fx/cooldown_clock.tscn) next to the energy cost of cards on Cooldown, with the number of
/// turns left inside it. Sits on the lower-right edge of the energy orb so it doesn't overlap the Novel butterfly.
/// Refreshed from <see cref="CooldownGrayscalePortraitPatch"/> (end of Reload and UpdateVisuals).
/// </summary>
public static class CooldownClock
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "cooldown_clock.tscn");
    private const string NodeName = "CooldownClock";

    private static readonly AccessTools.FieldRef<NCard, TextureRect> EnergyIconRef =
        AccessTools.FieldRefAccess<NCard, TextureRect>("_energyIcon");

    public static void Refresh(NCard card)
    {
        var icon = EnergyIconRef(card);
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return; // Not ready yet
        var parent = icon.GetParent();
        if (parent == null) return;

        var turns = card.Model is { } model && card.Visibility == ModelVisibility.Visible
            ? CooldownLook.TurnsLeft(model)
            : 0;

        var clock = parent.GetNodeOrNull<Node2D>(NodeName);
        if (turns <= 0)
        {
            if (clock != null) clock.Visible = false;
            return;
        }

        if (clock == null)
        {
            clock = GD.Load<PackedScene>(ScenePath).Instantiate<Node2D>();
            clock.Name = NodeName;
            parent.AddChild(clock);
        }

        clock.Position = icon.Position + new Vector2(icon.Size.X * 0.95f, icon.Size.Y * 0.8f);
        clock.Call("set_turns", turns);
        clock.Visible = true;
    }
}
