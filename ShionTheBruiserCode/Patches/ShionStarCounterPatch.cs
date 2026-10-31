using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Shion's star counter uses her own icon (images/ui/stars/shion_stars_icon.png) instead of the Regent's star.
/// The rotating star layers are hidden. The icon keeps its HSV material, so it still dims at 0 Stars and
/// flashes on gain. Only the local player's counter is affected, so this is cosmetic.
/// Patches NCombatUi.Activate rather than NStarCounter.Initialize: Initialize is tiny and gets inlined, so a
/// postfix on it never ran.
/// </summary>
[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
public static class ShionStarCounterPatch
{
    private static readonly string IconPath = "ui/stars/shion_stars_icon.png".ImagePath();

    // Pixels trimmed from each side of the 128px icon box (the round glow reads bigger than the vanilla star).
    private const float ShionIconInset = 22f;

    private const string OriginalTextureMeta = "shion_original_star_texture";

    private static readonly AccessTools.FieldRef<NCombatUi, NStarCounter> StarCounterRef =
        AccessTools.FieldRefAccess<NCombatUi, NStarCounter>("_starCounter");

    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        try
        {
            var isShion = LocalContext.GetMe(state)?.Character is Character.ShionTheBruiser;
            var icon = StarCounterRef(__instance).GetNode<TextureRect>("Icon");

            // Remember the vanilla star, in case this UI is reused for a non-Shion run later.
            if (!icon.HasMeta(OriginalTextureMeta)) icon.SetMeta(OriginalTextureMeta, icon.Texture);

            icon.Texture = isShion
                ? ResourceLoader.Load<Texture2D>(IconPath)
                : icon.GetMeta(OriginalTextureMeta).As<Texture2D>();
            // Icon is anchored full-rect with keep-aspect-centered stretch, so insetting its offsets shrinks it in place.
            var inset = isShion ? ShionIconInset : 0f;
            icon.OffsetLeft = inset;
            icon.OffsetTop = inset;
            icon.OffsetRight = -inset;
            icon.OffsetBottom = -inset;
            foreach (var layer in icon.GetNode<Control>("RotationLayers").GetChildren().OfType<CanvasItem>())
            {
                layer.Visible = !isShion;
            }
        }
        catch (Exception e)
        {
            Log.Error("Shion star counter icon failed: " + e.Message);
        }
    }
}
