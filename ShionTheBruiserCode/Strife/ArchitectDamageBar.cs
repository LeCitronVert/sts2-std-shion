using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// The Act 4 Architect's HP is infinite: instead of his health bar, the total damage he took this fight is shown.
/// Visual only.
/// </summary>
[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.RefreshValues))]
public static class ArchitectDamageBarPatch
{
    private const string LabelName = "StrifeArchitectDamage";

    private static readonly AccessTools.FieldRef<NHealthBar, Creature> CreatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");
    private static readonly AccessTools.FieldRef<NHealthBar, MegaLabel> HpLabelRef =
        AccessTools.FieldRefAccess<NHealthBar, MegaLabel>("_hpLabel");

    private static readonly Color TextColor = new(1f, 0.86f, 0.35f);
    private static readonly Color OutlineColor = new(0.18f, 0.1f, 0f);

    [HarmonyPostfix]
    public static void Postfix(NHealthBar __instance)
    {
        if (CreatureRef(__instance) is not { Monster: StrifeArchitect architect }) return;
        if (__instance.HpBarContainer is { } bar) bar.Modulate = Colors.Transparent;

        var label = __instance.GetNodeOrNull<Label>(LabelName);
        if (label == null)
        {
            label = CreateLabel(__instance);
            architect.DamageChanged += _ =>
            {
                if (GodotObject.IsInstanceValid(label)) Callable.From(() => SetText(label, architect)).CallDeferred();
            };
        }
        SetText(label, architect);
    }

    private static void SetText(Label label, StrifeArchitect architect)
    {
        var text = new LocString("monsters", "SHIONTHEBRUISER-STRIFE_ARCHITECT.DAMAGE_COUNTER");
        text.Add("Damage", architect.TotalDamage);
        label.Text = text.GetFormattedText();
    }

    private static Label CreateLabel(NHealthBar bar)
    {
        var label = new Label
        {
            Name = LabelName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorLeft = 0f,
            AnchorRight = 1f,
            AnchorTop = 0f,
            AnchorBottom = 1f,
            OffsetLeft = -80f,
            OffsetRight = 80f,
        };
        if (HpLabelRef(bar) is { } hpLabel)
        {
            if (hpLabel.GetThemeFont("font") is { } font) label.AddThemeFontOverride("font", font);
            label.AddThemeFontSizeOverride("font_size", Math.Max(22, hpLabel.GetThemeFontSize("font_size") + 4));
        }
        label.AddThemeColorOverride("font_color", TextColor);
        label.AddThemeColorOverride("font_outline_color", OutlineColor);
        label.AddThemeConstantOverride("outline_size", 10);
        bar.AddChild(label);
        return label;
    }
}
