using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Main menu overlay listing the seven Chaos Emeralds; locked ones are shown as dark silhouettes.
/// Built from plain Godot nodes so it doesn't need script registration.
/// </summary>
public static class ChaosEmeraldsScreen
{
    private const string Table = "main_menu_ui";
    private const int IconSize = 128;

    private static readonly Color LockedModulate = new(0.08f, 0.08f, 0.08f, 0.9f);

    public static Control Create(Font? font)
    {
        var root = new Control { Name = "ChaosEmeraldsScreen", MouseFilter = Control.MouseFilterEnum.Stop, ZIndex = 100 };
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var dim = new ColorRect { Color = new Color(0, 0, 0, 0.8f), MouseFilter = Control.MouseFilterEnum.Stop };
        dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        dim.GuiInput += e =>
        {
            if (e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) Close(root);
        };
        root.AddChild(dim);

        var center = new CenterContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(center);

        var column = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        column.AddThemeConstantOverride("separation", 32);
        center.AddChild(column);

        column.AddChild(MakeLabel(Loc("SHIONTHEBRUISER-CHAOS_EMERALDS_SCREEN.title"), font, 56));

        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 24);
        column.AddChild(row);

        foreach (var color in ChaosEmeraldHelper.AllColors)
        {
            row.AddChild(MakeEmeraldSlot(color, font));
        }

        var unlockedCount = ChaosEmeraldHelper.AllColors.Count(ChaosEmeraldHelper.IsUnlocked);
        var progress = new LocString(Table, "SHIONTHEBRUISER-CHAOS_EMERALDS_SCREEN.progress");
        progress.Add("Unlocked", unlockedCount);
        progress.Add("Total", ChaosEmeraldHelper.AllColors.Length);
        column.AddChild(MakeLabel(progress.GetFormattedText(), font, 32));

        var back = new Button
        {
            Text = Loc("SHIONTHEBRUISER-CHAOS_EMERALDS_SCREEN.back"),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(240, 64),
        };
        if (font != null) back.AddThemeFontOverride("font", font);
        back.AddThemeFontSizeOverride("font_size", 32);
        back.Pressed += () => Close(root);
        column.AddChild(back);

        return root;
    }

    private static Control MakeEmeraldSlot(Enums.ChaosEmeraldColor color, Font? font)
    {
        var unlocked = ChaosEmeraldHelper.IsUnlocked(color);
        var relic = ChaosEmeraldHelper.SoloRelic(color);

        var slot = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        slot.AddThemeConstantOverride("separation", 8);

        var icon = new TextureRect
        {
            Texture = relic.Icon,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(IconSize, IconSize),
            Modulate = unlocked ? Colors.White : LockedModulate,
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        slot.AddChild(icon);

        IEnumerable<IHoverTip> tips = unlocked
            ? relic.HoverTips
            : [new HoverTip(new LocString(Table, "SHIONTHEBRUISER-CHAOS_EMERALDS_SCREEN.locked_title"),
                new LocString(Table, "SHIONTHEBRUISER-CHAOS_EMERALDS_SCREEN.locked_description"))];
        icon.MouseEntered += () => NHoverTipSet.CreateAndShow(icon, tips, HoverTipAlignment.Right);
        icon.MouseExited += () => NHoverTipSet.Remove(icon);

        var name = unlocked ? relic.Title.GetFormattedText() : Loc("SHIONTHEBRUISER-CHAOS_EMERALDS_SCREEN.locked_title");
        slot.AddChild(MakeLabel(name, font, 22));
        return slot;
    }

    private static Label MakeLabel(string text, Font? font, int size)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        if (font != null) label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static string Loc(string key) => new LocString(Table, key).GetFormattedText();

    private static void Close(Control root)
    {
        NHoverTipSet.Clear();
        root.QueueFree();
    }
}
