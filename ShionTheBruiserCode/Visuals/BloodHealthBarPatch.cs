using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Blood is shown on the health bar instead of as a power icon: a dark scarlet segment right after the current HP,
/// in the missing HP part of the bar (Blood is capped by missing HP, so it always fits), and the amount in scarlet
/// just right of the bar. Hovering the creature still shows Blood's hover tip.
/// Visual only.
/// </summary>
[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.RefreshValues))]
public static class BloodHealthBarPatch
{
    private const string SegmentName = "SeelsBloodSegment";
    private const string LabelName = "SeelsBloodLabel";

    /// <summary>Same minimum width as the game's foreground (NHealthBar._minSize).</summary>
    private const float MinHpWidth = 12f;

    private const float LabelGap = 6f;

    private static readonly Color SegmentColor = new(0.55f, 0.02f, 0.09f, 0.95f);
    private static readonly Color LabelColor = new(1f, 0.25f, 0.3f);
    private static readonly Color LabelOutlineColor = new(0.2f, 0f, 0.02f);

    private static readonly AccessTools.FieldRef<NHealthBar, Creature> CreatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> ForegroundContainerRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_hpForegroundContainer");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> ForegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_hpForeground");
    private static readonly AccessTools.FieldRef<NHealthBar, MegaLabel> HpLabelRef =
        AccessTools.FieldRefAccess<NHealthBar, MegaLabel>("_hpLabel");
    private static readonly Func<NHealthBar, float> MaxFgWidth =
        AccessTools.MethodDelegate<Func<NHealthBar, float>>(AccessTools.PropertyGetter(typeof(NHealthBar), "MaxFgWidth"));

    [HarmonyPostfix]
    public static void Postfix(NHealthBar __instance)
    {
        var creature = CreatureRef(__instance);
        var container = ForegroundContainerRef(__instance);
        if (creature is null || container is null || !GodotObject.IsInstanceValid(container)) return;

        var blood = creature.IsAlive ? SeelsMechanicsHelper.GetBlood(creature) : 0;
        var segment = container.GetNodeOrNull<ColorRect>(SegmentName);
        var label = __instance.HpBarContainer?.GetNodeOrNull<Label>(LabelName);

        if (blood <= 0 || creature.MaxHp <= 0)
        {
            if (segment != null) segment.Visible = false;
            if (label != null) label.Visible = false;
            return;
        }

        segment ??= CreateSegment(container, ForegroundRef(__instance));
        label ??= CreateLabel(__instance);

        // Same width the game uses for the HP foreground
        var width = MaxFgWidth(__instance);
        var hpWidth = Math.Max((float)creature.CurrentHp / creature.MaxHp * width, MinHpWidth);
        var bloodEnd = Math.Min((float)(creature.CurrentHp + blood) / creature.MaxHp * width, width);

        segment.Visible = bloodEnd > hpWidth;
        segment.OffsetLeft = hpWidth;
        segment.OffsetRight = bloodEnd;

        if (label != null)
        {
            label.Visible = true;
            label.Text = blood.ToString();
        }
    }

    private static ColorRect CreateSegment(Control container, Control foreground)
    {
        var segment = new ColorRect
        {
            Name = SegmentName,
            Color = SegmentColor,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            // Left-anchored; the horizontal offsets are set on every refresh. Same height as the HP foreground.
            AnchorLeft = 0f,
            AnchorRight = 0f,
            AnchorTop = foreground.AnchorTop,
            AnchorBottom = foreground.AnchorBottom,
            OffsetTop = foreground.OffsetTop,
            OffsetBottom = foreground.OffsetBottom,
        };
        container.AddChild(segment);
        // Right after the HP foreground, so the poison / doom overlays still draw over it
        container.MoveChild(segment, foreground.GetIndex() + 1);
        return segment;
    }

    private static Label? CreateLabel(NHealthBar bar)
    {
        var hpLabel = HpLabelRef(bar);
        var barContainer = bar.HpBarContainer;
        if (barContainer is null || hpLabel is null) return null;

        var label = new Label
        {
            Name = LabelName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            // Just right of the bar, as tall as it
            AnchorLeft = 1f,
            AnchorRight = 1f,
            AnchorTop = 0f,
            AnchorBottom = 1f,
            OffsetLeft = LabelGap,
            OffsetRight = LabelGap + 60f,
            OffsetTop = -8f,
            OffsetBottom = 8f,
        };
        var font = hpLabel.GetThemeFont("font");
        if (font != null) label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", Math.Max(16, hpLabel.GetThemeFontSize("font_size")));
        label.AddThemeColorOverride("font_color", LabelColor);
        label.AddThemeColorOverride("font_outline_color", LabelOutlineColor);
        label.AddThemeConstantOverride("outline_size", 8);
        barContainer.AddChild(label);
        return label;
    }
}

/// <summary>Blood's icon isn't added to the creature's power row (it's on the health bar instead).</summary>
[HarmonyPatch(typeof(NPowerContainer), "Add")]
public static class BloodPowerIconPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PowerModel power) => power is not Blood;
}
