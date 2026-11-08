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
/// Blood is shown on the health bar instead of as a power icon, as a health bar segment like Poison / Doom:
/// a copy of the game's Poison segment (same nine-patch fill texture, inside the bar's mask) tinted dark scarlet,
/// right after the current HP, in the missing HP part of the bar (Blood is capped by missing HP, so it always fits).
/// It tucks under the HP / Poison segments' rounded edge like Poison does under HP, and under the orange damage trail.
/// The amount is in scarlet just right of the bar. Hovering the creature still shows Blood's hover tip.
/// Visual only.
/// </summary>
[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.RefreshValues))]
public static class BloodHealthBarPatch
{
    private const string SegmentName = "SeelsBloodForeground";
    private const string LabelName = "SeelsBloodLabel";

    /// <summary>Same minimum width as the game's foreground (NHealthBar._minSize).</summary>
    private const float MinHpWidth = 12f;

    private const float LabelGap = 6f;

    /// <summary>Poison's self_modulate is (0.47, 0.75, 0.24) and HP's is (0.8, 0.05, 0.05): Blood is a darker crimson.</summary>
    private static readonly Color SegmentColor = new(0.42f, 0.0f, 0.1f);
    private static readonly Color LabelColor = new(1f, 0.25f, 0.3f);
    private static readonly Color LabelOutlineColor = new(0.2f, 0f, 0.02f);

    private static readonly AccessTools.FieldRef<NHealthBar, Creature> CreatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> PoisonForegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_poisonForeground");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> MiddlegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_hpMiddleground");
    private static readonly AccessTools.FieldRef<NHealthBar, MegaLabel> HpLabelRef =
        AccessTools.FieldRefAccess<NHealthBar, MegaLabel>("_hpLabel");
    private static readonly Func<NHealthBar, float> MaxFgWidth =
        AccessTools.MethodDelegate<Func<NHealthBar, float>>(AccessTools.PropertyGetter(typeof(NHealthBar), "MaxFgWidth"));

    [HarmonyPostfix]
    public static void Postfix(NHealthBar __instance)
    {
        var creature = CreatureRef(__instance);
        var poison = PoisonForegroundRef(__instance);
        if (creature is null || poison is null || !GodotObject.IsInstanceValid(poison)) return;
        var mask = poison.GetParent();
        if (mask is null) return;

        var blood = creature.IsAlive && !creature.HpDisplay.IsInfinite() ? SeelsMechanicsHelper.GetBlood(creature) : 0;
        var segment = mask.GetNodeOrNull<NinePatchRect>(SegmentName);
        var label = __instance.HpBarContainer?.GetNodeOrNull<Label>(LabelName);

        if (blood <= 0 || creature.MaxHp <= 0 || creature.CurrentHp <= 0)
        {
            if (segment != null) segment.Visible = false;
            if (label != null) label.Visible = false;
            return;
        }

        segment ??= CreateSegment(__instance, poison, mask);
        label ??= CreateLabel(__instance);

        // Same maths as NHealthBar.GetFgWidth; offsets are relative to the full-width anchors, like Poison / Doom's.
        var width = MaxFgWidth(__instance);
        var hpEnd = Math.Max((float)creature.CurrentHp / creature.MaxHp * width, MinHpWidth);
        var bloodEnd = Math.Min(Math.Max((float)(creature.CurrentHp + blood) / creature.MaxHp * width, MinHpWidth), width);

        segment.Visible = bloodEnd > hpEnd;
        // Starts under the HP segment's rounded right edge, like Poison under HP
        segment.OffsetLeft = Math.Max(0f, hpEnd - segment.PatchMarginLeft);
        segment.OffsetRight = bloodEnd - width;

        if (label != null)
        {
            label.Visible = true;
            label.Text = blood.ToString();
        }
    }

    private static NinePatchRect CreateSegment(NHealthBar bar, Control poison, Node mask)
    {
        // Copy of the Poison segment: same texture, patch margins, anchors and height
        var segment = (NinePatchRect)poison.Duplicate((int)(Node.DuplicateFlags.Groups | Node.DuplicateFlags.UseInstantiation));
        segment.Name = SegmentName;
        segment.UniqueNameInOwner = false;
        segment.SelfModulate = SegmentColor;
        segment.Modulate = Colors.White;
        segment.MouseFilter = Control.MouseFilterEnum.Ignore;
        segment.AnchorLeft = 0f;
        segment.AnchorRight = 1f;
        mask.AddChild(segment);
        // Drawn first in the mask: under the orange damage trail (so it shows once the trail settles) and under the
        // HP / Poison / Doom segments
        var middleground = MiddlegroundRef(bar);
        mask.MoveChild(segment, middleground != null && middleground.GetParent() == mask ? middleground.GetIndex() : 0);
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
