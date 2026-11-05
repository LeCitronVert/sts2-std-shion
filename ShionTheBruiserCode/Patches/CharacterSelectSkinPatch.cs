using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Forum style skin for the character select info panel of our characters (ICharacterSelectSkin):
/// semi-transparent backing, the class name above the first name and HP/Gold in a column on its right,
/// the starting relic's icon centred on its text,
/// and playstyle coins in rows of 3 with their own tooltip.
/// The vanilla panel is shared by every character, so everything is added on top of it and undone on the next selection;
/// vanilla characters, the random button and the character buttons themselves are left untouched.
/// Gamepad: up from the character buttons enters the coin row, left/right moves, down goes back.
/// </summary>
[HarmonyPatch]
public static class CharacterSelectSkinPatch
{
    private static readonly Color Ink = new("120d03");
    private static readonly Color GoldLine = new("e2bf3c");
    private static readonly Color GoldText = new("f5c21c");
    private static readonly Color CoinFill = new("4a0805");
    private static readonly Color BodyText = new("fff6e2");
    /// <summary>Clean semi-transparent backing for the panel.</summary>
    private static readonly Color PanelBg = new(0.07f, 0.05f, 0.015f, 0.72f);
    private static readonly Color TipBg = new(0.07f, 0.05f, 0.015f, 0.94f);
    private static readonly Color SubtleLine = new(0.886f, 0.749f, 0.235f, 0.35f);

    /// <summary>Space between the panel content and the edge of the backing.</summary>
    private const float FramePad = 28f;
    private const int FrameRadius = 10;
    private const float CoinSize = 64f;
    private const int CoinColumns = 3;
    private const int CoinGap = 14;
    /// <summary>HP/Gold column: top edge relative to the bottom of the name (centres two 40px rows on it).</summary>
    private const float HpGoldTop = -131f;
    private const int HpGoldSeparation = 14;
    /// <summary>Room between the HP/Gold column and the right edge of the backing, for the chips' padding.</summary>
    private const float HpGoldRightInset = 18f;
    private const float TipWidth = 380f;

    private sealed class SkinState
    {
        public bool Active;
        public bool Hooked;
        public bool Captured;
        public readonly List<Node> Added = [];
        public readonly List<Button> Coins = [];
        public readonly Dictionary<Control, NodePath> OriginalTops = [];
        public Control? Frame;
        /// <summary>Narrow backing under the main one, only as wide as the coin grid (keeps clear of the ascension panel).</summary>
        public Control? FrameBottom;
        public Control? Section;
        public Control? Grid;
        public Control? HpChip;
        public Control? GoldChip;
        public PanelContainer? Tip;
        public Label? TipTitle;
        public RichTextLabel? TipBody;
        public Control? FocusOrigin;
        public IReadOnlyList<CharacterPlaystyle> Playstyles = [];
        public CharacterModel? Character;

        // Vanilla values restored on Revert
        public Color NameColor, NameOutlineColor;
        public int NameOutlineSize;
        public VerticalAlignment NameVAlign;
        public bool NinePatchVisible;
        public Vector2 SpacerMinSize;
        public float HpGoldTop, HpGoldBottom, HpGoldLeft, HpGoldRight, HpGoldAnchorLeft, HpGoldAnchorRight;
        public Control.GrowDirection HpGoldGrow;
        public bool HpGoldVertical;
        public int HpGoldSeparation;
        public float RelicIconTop, RelicIconBottom;
    }

    private static readonly ConditionalWeakTable<NCharacterSelectScreen, SkinState> States = new();

    // The screen's fields aren't publicized, so nodes are found by their scene paths (same ones NCharacterSelectScreen._Ready uses)
    private static Control InfoPanel(NCharacterSelectScreen screen) => screen.GetNode<Control>("InfoPanel");

    // ───────────────────────────── Hooks ─────────────────────────────

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)] // after BaseLib's own SelectCharacter postfixes
    public static void AfterSelectCharacter(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton,
        CharacterModel characterModel)
    {
        try
        {
            Revert(__instance, restoreFocus: false);
            if (charSelectButton.IsRandom || characterModel is not ICharacterSelectSkin skin) return;
            Apply(__instance, characterModel, skin, charSelectButton, charSelectButton.IsLocked);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Character select skin failed for {characterModel.Id}: {e}");
            try { Revert(__instance, restoreFocus: true); }
            catch (Exception inner) { MainFile.Logger.Error($"Character select skin revert failed: {inner}"); }
        }
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
    [HarmonyPrefix]
    public static void BeforeSubmenuOpened(NCharacterSelectScreen __instance) => Revert(__instance, restoreFocus: false);

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuClosed))]
    [HarmonyPrefix]
    public static void BeforeSubmenuClosed(NCharacterSelectScreen __instance) => Revert(__instance, restoreFocus: false);

    /// <summary>Embark is bound to the select button: if a coin has focus, hand it back to the character button first.</summary>
    [HarmonyPatch(typeof(NCharacterSelectScreen), "OnEmbarkPressed")]
    [HarmonyPrefix]
    public static void BeforeEmbark(NCharacterSelectScreen __instance)
    {
        if (!States.TryGetValue(__instance, out var s) || !s.Active) return;
        if (CoinHasFocus(__instance, s)) RestoreFocus(s);
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), "OnEmbarkPressed")]
    [HarmonyPostfix]
    public static void AfterEmbark(NCharacterSelectScreen __instance)
    {
        // Vanilla disables the back button once the local player is ready (not when the tutorial popup interrupts)
        if (!(__instance.GetNodeOrNull<NClickableControl>("BackButton")?.IsEnabled ?? true)) SetCoinsEnabled(__instance, false);
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), "OnUnreadyPressed")]
    [HarmonyPostfix]
    public static void AfterUnready(NCharacterSelectScreen __instance) => SetCoinsEnabled(__instance, true);

    // ───────────────────────────── Apply ─────────────────────────────

    private static void Apply(NCharacterSelectScreen screen, CharacterModel character, ICharacterSelectSkin skin,
        NCharacterSelectButton button, bool locked)
    {
        var s = States.GetOrCreateValue(screen);
        var panel = InfoPanel(screen);
        var vbox = panel.GetNode<VBoxContainer>("VBoxContainer");
        var spacer = vbox.GetNode<Control>("HpGoldSpacer");
        var hpGold = spacer.GetNode<BoxContainer>("HpGold");
        var relic = vbox.GetNode<Control>("Relic");
        var nine = panel.GetNodeOrNull<CanvasItem>("NinePatchRect");
        Label name = screen.GetNode<Label>("InfoPanel/VBoxContainer/Name");

        if (!s.Captured)
        {
            s.NameColor = name.GetThemeColor("font_color");
            s.NameOutlineColor = name.GetThemeColor("font_outline_color");
            s.NameOutlineSize = name.GetThemeConstant("outline_size");
            s.NameVAlign = name.VerticalAlignment;
            s.NinePatchVisible = nine?.Visible ?? true;
            s.SpacerMinSize = spacer.CustomMinimumSize;
            s.HpGoldTop = hpGold.OffsetTop;
            s.HpGoldBottom = hpGold.OffsetBottom;
            s.HpGoldLeft = hpGold.OffsetLeft;
            s.HpGoldRight = hpGold.OffsetRight;
            s.HpGoldAnchorLeft = hpGold.AnchorLeft;
            s.HpGoldAnchorRight = hpGold.AnchorRight;
            s.HpGoldGrow = hpGold.GrowHorizontal;
            s.HpGoldVertical = hpGold.Vertical;
            s.HpGoldSeparation = hpGold.GetThemeConstant("separation");
            var relicIcon = relic.GetNode<Control>("Icon");
            s.RelicIconTop = relicIcon.OffsetTop;
            s.RelicIconBottom = relicIcon.OffsetBottom;
            s.Captured = true;
        }

        s.Active = true;
        s.Character = character;
        s.FocusOrigin = button;

        if (!s.Hooked)
        {
            // Description length changes the panel height: keep the frame and chips fitted to it
            vbox.Connect(Container.SignalName.SortChildren, Callable.From(() => Refit(screen)));
            ((Container)hpGold).Connect(Container.SignalName.SortChildren, Callable.From(() => Refit(screen)));
            s.Hooked = true;
        }

        if (nine != null) nine.Visible = false;

        bool hasPlaystyles = !locked && skin.Playstyles.Count > 0;

        // Backing: plain semi-transparent rounded panel. With playstyles, a narrower part hangs under it,
        // only as wide as the coin grid, so the panel stays clear of the ascension controls.
        var frame = Track(s, Box("ShionTheBruiserSkinFrame", PanelBg, Colors.Transparent, 0));
        SetCorners(frame, FrameRadius, FrameRadius, FrameRadius, hasPlaystyles ? 0 : FrameRadius);
        panel.AddChild(frame);
        panel.MoveChild(frame, 0);
        s.Frame = frame;
        if (hasPlaystyles)
        {
            var frameBottom = Track(s, Box("ShionTheBruiserSkinFrameBottom", PanelBg, Colors.Transparent, 0));
            SetCorners(frameBottom, 0, 0, FrameRadius, FrameRadius);
            panel.AddChild(frameBottom);
            panel.MoveChild(frameBottom, 1);
            s.FrameBottom = frameBottom;
        }

        // Name: white outlined first name, class name chip above it, HP/Gold on its right
        name.AddThemeColorOverride("font_color", Colors.White);
        name.AddThemeColorOverride("font_outline_color", Colors.Black);
        name.AddThemeConstantOverride("outline_size", 22);
        name.VerticalAlignment = VerticalAlignment.Bottom;


        var classKey = $"{character.Id.Entry}.className";
        if (!locked && LocString.Exists("characters", classKey))
        {
            var chip = new PanelContainer { Name = "ShionTheBruiserSkinClass", MouseFilter = Control.MouseFilterEnum.Ignore };
            chip.AddThemeStyleboxOverride("panel", Style(Ink, Colors.Black, 2, padX: 10, padY: 3));
            chip.AddChild(SmallCaps(screen, new LocString("characters", classKey).GetFormattedText(), 18));
            chip.Position = new Vector2(0, 6);
            name.AddChild(Track(s, chip));
        }

        // HP/Gold: a column at the right of the name, on black chips.
        // HpGold lives in the spacer right under the name, so negative offsets lift it back up beside it.
        hpGold.Vertical = true;
        hpGold.AddThemeConstantOverride("separation", HpGoldSeparation);
        hpGold.AnchorLeft = 1;
        hpGold.AnchorRight = 1;
        hpGold.GrowHorizontal = Control.GrowDirection.Begin; // grows leftwards from the right edge to fit "65/65"
        hpGold.OffsetLeft = 0;
        hpGold.OffsetRight = -HpGoldRightInset;
        hpGold.OffsetTop = HpGoldTop;
        hpGold.OffsetBottom = HpGoldTop;
        s.HpChip = Track(s, Box("ShionTheBruiserSkinHpChip", Colors.Black, GoldLine, 2));
        s.GoldChip = Track(s, Box("ShionTheBruiserSkinGoldChip", Colors.Black, GoldLine, 2));
        spacer.AddChild(s.HpChip);
        spacer.AddChild(s.GoldChip);
        spacer.MoveChild(s.HpChip, 0);
        spacer.MoveChild(s.GoldChip, 1);

        // Relic: faint gold lines above and below the row (the icon is centred on its text in Refit)
        relic.AddChild(Track(s, Line("ShionTheBruiserSkinRelicTop", top: true)));
        relic.AddChild(Track(s, Line("ShionTheBruiserSkinRelicBottom", top: false)));

        if (hasPlaystyles) BuildPlaystyles(screen, s, vbox, panel, character, skin.Playstyles, button);

        Refit(screen);
        Callable.From(() => Refit(screen)).CallDeferred();
        // The relic description's wrapped height is only known once it has been drawn
        screen.GetTree().CreateTimer(0.05).Timeout += () => Refit(screen);
    }

    private static void BuildPlaystyles(NCharacterSelectScreen screen, SkinState s, VBoxContainer vbox, Control panel,
        CharacterModel character, IReadOnlyList<CharacterPlaystyle> playstyles, NCharacterSelectButton origin)
    {
        s.Playstyles = playstyles;

        var section = new VBoxContainer { Name = "ShionTheBruiserSkinPlaystyles", MouseFilter = Control.MouseFilterEnum.Ignore };
        section.AddThemeConstantOverride("separation", 10);
        section.AddChild(new Control { CustomMinimumSize = new Vector2(0, 4), MouseFilter = Control.MouseFilterEnum.Ignore });
        var header = SmallCaps(screen, new LocString("main_menu_ui", "SHIONTHEBRUISER-CHARACTER_SELECT.playstyles").GetFormattedText(), 18);
        header.AddThemeColorOverride("font_color", GoldText);
        section.AddChild(header);

        // Wraps every CoinColumns coins, so the row never reaches the ascension controls
        var row = new GridContainer
        {
            Name = "Coins",
            Columns = CoinColumns,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
        };
        row.AddThemeConstantOverride("h_separation", CoinGap);
        row.AddThemeConstantOverride("v_separation", CoinGap);
        section.AddChild(row);
        s.Section = section;
        s.Grid = row;

        for (int i = 0; i < playstyles.Count; i++)
        {
            int index = i;
            var coin = new Button
            {
                Name = $"Coin{i}",
                CustomMinimumSize = new Vector2(CoinSize, CoinSize),
                FocusMode = Control.FocusModeEnum.All,
                MouseFilter = Control.MouseFilterEnum.Stop,
            };
            SetCoinStyle(coin, active: false);
            coin.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());

            var texture = LoadIcon(playstyles[i]);
            if (texture != null)
            {
                coin.AddChild(FullRect(new TextureRect
                {
                    Name = "Icon",
                    Texture = texture,
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                }, 9));
            }

            coin.MouseEntered += () => ShowTip(screen, index);
            coin.FocusEntered += () => ShowTip(screen, index);
            coin.MouseExited += () => { if (!coin.HasFocus()) HideTip(screen); };
            coin.FocusExited += () => HideTip(screen);

            row.AddChild(coin);
            s.Coins.Add(coin);
        }

        vbox.AddChild(Track(s, section));

        // Tooltip, drawn over the panel
        var tip = new PanelContainer { Name = "ShionTheBruiserSkinTip", Visible = false, MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 10 };
        var tipStyle = Style(TipBg, SubtleLine, 1, padX: 0, padY: 0);
        tipStyle.SetCornerRadiusAll(8);
        tipStyle.ShadowColor = new Color(0, 0, 0, 0.4f);
        tipStyle.ShadowSize = 8;
        tip.AddThemeStyleboxOverride("panel", tipStyle);
        tip.CustomMinimumSize = new Vector2(TipWidth, 0);

        var tipBox = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        tipBox.AddThemeConstantOverride("separation", 0);
        tip.AddChild(tipBox);

        var titleMargin = Margin(12, 7);
        var title = new Label { MouseFilter = Control.MouseFilterEnum.Ignore };
        title.AddThemeFontOverride("font", screen.GetNode<Control>("InfoPanel/VBoxContainer/Relic/Name/RichTextLabel").GetThemeFont("normal_font"));
        title.AddThemeFontSizeOverride("font_size", 24);
        title.AddThemeColorOverride("font_color", GoldText);
        titleMargin.AddChild(title);
        tipBox.AddChild(titleMargin);
        tipBox.AddChild(new ColorRect { Color = SubtleLine, CustomMinimumSize = new Vector2(0, 1), MouseFilter = Control.MouseFilterEnum.Ignore });

        var bodyMargin = Margin(12, 10);
        var body = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(TipWidth - 24, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        body.AddThemeFontOverride("normal_font", screen.GetNode<Control>("InfoPanel/VBoxContainer/Relic/Description").GetThemeFont("normal_font"));
        body.AddThemeFontOverride("bold_font", screen.GetNode<Control>("InfoPanel/VBoxContainer/Relic/Description").GetThemeFont("bold_font"));
        body.AddThemeFontSizeOverride("normal_font_size", 21);
        body.AddThemeFontSizeOverride("bold_font_size", 21);
        body.AddThemeColorOverride("default_color", BodyText);
        bodyMargin.AddChild(body);
        tipBox.AddChild(bodyMargin);

        panel.AddChild(Track(s, tip));
        s.Tip = tip;
        s.TipTitle = title;
        s.TipBody = body;

        // Gamepad: character buttons ↑ the bottom coin row, up/down moves between rows,
        // left/right runs through every coin in order, ↓ from the bottom row goes back to the selected character
        int count = s.Coins.Count;
        int lastRowStart = (count - 1) / CoinColumns * CoinColumns;
        foreach (var charButton in screen.GetNode<Control>("CharSelectButtons/ButtonContainer").GetChildren().OfType<Control>().Where(c => c.Visible))
        {
            s.OriginalTops[charButton] = charButton.FocusNeighborTop;
            charButton.FocusNeighborTop = s.Coins[lastRowStart].GetPath();
        }
        for (int i = 0; i < count; i++)
        {
            var coin = s.Coins[i];
            coin.FocusNeighborTop = (i >= CoinColumns ? s.Coins[i - CoinColumns] : coin).GetPath();
            // From a short last row, the coin above-right falls through to the last coin
            coin.FocusNeighborBottom = i + CoinColumns < count ? s.Coins[i + CoinColumns].GetPath()
                : i < lastRowStart ? s.Coins[count - 1].GetPath()
                : origin.GetPath();
            coin.FocusNeighborLeft = s.Coins[Math.Max(i - 1, 0)].GetPath();
            coin.FocusNeighborRight = s.Coins[Math.Min(i + 1, count - 1)].GetPath();
            coin.FocusPrevious = coin.FocusNeighborLeft;
            coin.FocusNext = coin.FocusNeighborRight;
        }
    }

    // ───────────────────────────── Tooltip ─────────────────────────────

    private static void ShowTip(NCharacterSelectScreen screen, int index)
    {
        if (!States.TryGetValue(screen, out var s) || !s.Active || s.Tip == null || s.Character == null) return;
        if (index < 0 || index >= s.Playstyles.Count) return;

        for (int i = 0; i < s.Coins.Count; i++) SetCoinStyle(s.Coins[i], i == index);

        var key = $"{s.Character.Id.Entry}.playstyle.{s.Playstyles[index].Key}";
        s.TipTitle!.Text = Loc($"{key}.title");
        s.TipBody!.Text = Loc($"{key}.description");
        s.Tip.Visible = true;
        s.Tip.ResetSize();

        // Fit the text first, then place it next frame once the size is known
        var coin = s.Coins[index];
        Callable.From(() => PlaceTip(screen, coin)).CallDeferred();
        PlaceTip(screen, coin);
    }

    private static void PlaceTip(NCharacterSelectScreen screen, Button coin)
    {
        if (!States.TryGetValue(screen, out var s) || !s.Active || s.Tip == null) return;
        if (!GodotObject.IsInstanceValid(coin) || !GodotObject.IsInstanceValid(s.Tip) || !s.Tip.Visible) return;

        s.Tip.ResetSize();
        // Above the coin grid, lined up with its left edge: below or beside it would cover the ascension
        // controls and the character buttons
        var anchor = s.Grid != null && GodotObject.IsInstanceValid(s.Grid) ? s.Grid.GetGlobalRect() : coin.GetGlobalRect();
        s.Tip.GlobalPosition = new Vector2(anchor.Position.X, anchor.Position.Y - 14 - s.Tip.Size.Y);
    }

    private static void HideTip(NCharacterSelectScreen screen)
    {
        if (!States.TryGetValue(screen, out var s) || !s.Active) return;
        foreach (var coin in s.Coins) if (GodotObject.IsInstanceValid(coin)) SetCoinStyle(coin, active: false);
        if (s.Tip != null && GodotObject.IsInstanceValid(s.Tip)) s.Tip.Visible = false;
    }

    // ───────────────────────────── Layout ─────────────────────────────

    /// <summary>Fits the frame around the visible panel content and the chips behind HP/Gold.</summary>
    private static void Refit(NCharacterSelectScreen screen)
    {
        if (!States.TryGetValue(screen, out var s) || !s.Active) return;
        var vbox = InfoPanel(screen).GetNodeOrNull<VBoxContainer>("VBoxContainer");
        if (vbox == null) return;

        AlignRelicIcon(vbox);

        if (s.Frame != null && GodotObject.IsInstanceValid(s.Frame))
        {
            var children = vbox.GetChildren().OfType<Control>().Where(c => c.Visible).ToList();
            if (children.Count > 0)
            {
                float top = children.Min(c => c.Position.Y);
                float bottom = children.Max(c => c.Position.Y + c.Size.Y);
                var section = s.Section != null && GodotObject.IsInstanceValid(s.Section) ? s.Section : null;
                var grid = s.Grid != null && GodotObject.IsInstanceValid(s.Grid) ? s.Grid : null;
                bool split = section != null && grid != null && s.FrameBottom != null && GodotObject.IsInstanceValid(s.FrameBottom);

                // Main backing: everything down to the relic row (or all of it, without playstyles)
                float mainBottom = split ? section!.Position.Y : bottom + FramePad;
                s.Frame.Position = vbox.Position + new Vector2(-FramePad, top - FramePad);
                s.Frame.Size = new Vector2(vbox.Size.X + FramePad * 2, mainBottom - (top - FramePad));

                if (split)
                {
                    // Narrow backing under it, as wide as the coin grid (or the header, whichever is wider)
                    float width = section!.GetChildren().OfType<Control>().Max(c => c.Position.X + c.GetCombinedMinimumSize().X);
                    width = Math.Max(width, grid!.Position.X + grid.Size.X);
                    s.FrameBottom!.Position = vbox.Position + new Vector2(-FramePad, section.Position.Y);
                    s.FrameBottom.Size = new Vector2(width + FramePad * 2, section.Size.Y + FramePad);
                }
            }
        }

        var hpGold = vbox.GetNodeOrNull<Control>("HpGoldSpacer/HpGold");
        if (hpGold == null) return;
        FitChip(s.HpChip, hpGold, hpGold.GetNodeOrNull<Control>("Hp"));
        FitChip(s.GoldChip, hpGold, hpGold.GetNodeOrNull<Control>("Gold"));
    }

    /// <summary>Centres the relic icon on the relic's name + description block instead of hanging at its top.</summary>
    private static void AlignRelicIcon(VBoxContainer vbox)
    {
        var icon = vbox.GetNodeOrNull<Control>("Relic/Icon");
        var title = vbox.GetNodeOrNull<Control>("Relic/Name");
        var description = vbox.GetNodeOrNull<RichTextLabel>("Relic/Description");
        if (icon == null || title == null || description == null) return;

        float textTop = title.Position.Y + title.GetThemeConstant("margin_top");
        float textBottom = description.Position.Y + Math.Max(description.GetContentHeight(), 1);
        float iconTop = (textTop + textBottom) / 2f - icon.Size.Y / 2f;
        icon.Position = new Vector2(icon.Position.X, Math.Max(iconTop, 0));
    }

    private static void FitChip(Control? chip, Control hpGold, Control? target)
    {
        if (chip == null || target == null || !GodotObject.IsInstanceValid(chip)) return;
        chip.Position = hpGold.Position + target.Position + new Vector2(-10, -5);
        chip.Size = target.Size + new Vector2(28, 10);
    }

    // ───────────────────────────── Revert ─────────────────────────────

    private static void Revert(NCharacterSelectScreen screen, bool restoreFocus)
    {
        if (!States.TryGetValue(screen, out var s) || !s.Active) return;

        if (restoreFocus || CoinHasFocus(screen, s)) RestoreFocus(s);

        foreach (var (control, path) in s.OriginalTops)
            if (GodotObject.IsInstanceValid(control)) control.FocusNeighborTop = path;
        s.OriginalTops.Clear();

        foreach (var node in s.Added)
        {
            if (!GodotObject.IsInstanceValid(node)) continue;
            node.GetParent()?.RemoveChild(node);
            node.QueueFree();
        }
        s.Added.Clear();
        s.Coins.Clear();
        s.Frame = s.FrameBottom = s.Section = s.Grid = s.HpChip = s.GoldChip = null;
        s.Tip = null;
        s.TipTitle = null;
        s.TipBody = null;
        s.Playstyles = [];
        s.Character = null;
        s.FocusOrigin = null;

        Label name = screen.GetNode<Label>("InfoPanel/VBoxContainer/Name");
        name.AddThemeColorOverride("font_color", s.NameColor);
        name.AddThemeColorOverride("font_outline_color", s.NameOutlineColor);
        name.AddThemeConstantOverride("outline_size", s.NameOutlineSize);
        name.VerticalAlignment = s.NameVAlign;

        var panel = InfoPanel(screen);
        var nine = panel.GetNodeOrNull<CanvasItem>("NinePatchRect");
        if (nine != null) nine.Visible = s.NinePatchVisible;
        var spacer = panel.GetNodeOrNull<Control>("VBoxContainer/HpGoldSpacer");
        if (spacer != null) spacer.CustomMinimumSize = s.SpacerMinSize;
        var hpGold = panel.GetNodeOrNull<BoxContainer>("VBoxContainer/HpGoldSpacer/HpGold");
        if (hpGold != null)
        {
            hpGold.Vertical = s.HpGoldVertical;
            hpGold.AddThemeConstantOverride("separation", s.HpGoldSeparation);
            hpGold.AnchorLeft = s.HpGoldAnchorLeft;
            hpGold.AnchorRight = s.HpGoldAnchorRight;
            hpGold.GrowHorizontal = s.HpGoldGrow;
            hpGold.OffsetLeft = s.HpGoldLeft;
            hpGold.OffsetRight = s.HpGoldRight;
            hpGold.OffsetTop = s.HpGoldTop;
            hpGold.OffsetBottom = s.HpGoldBottom;
        }
        var relicIcon = panel.GetNodeOrNull<Control>("VBoxContainer/Relic/Icon");
        if (relicIcon != null)
        {
            relicIcon.OffsetTop = s.RelicIconTop;
            relicIcon.OffsetBottom = s.RelicIconBottom;
        }

        s.Active = false;
    }

    private static bool CoinHasFocus(NCharacterSelectScreen screen, SkinState s)
    {
        var owner = screen.GetViewport()?.GuiGetFocusOwner();
        return owner != null && s.Coins.Any(c => c == owner);
    }

    private static void RestoreFocus(SkinState s)
    {
        if (s.FocusOrigin != null && GodotObject.IsInstanceValid(s.FocusOrigin) && s.FocusOrigin.IsVisibleInTree()
            && s.FocusOrigin.FocusMode != Control.FocusModeEnum.None)
            s.FocusOrigin.GrabFocus();
    }

    private static void SetCoinsEnabled(NCharacterSelectScreen screen, bool enabled)
    {
        if (!States.TryGetValue(screen, out var s) || !s.Active) return;
        if (!enabled) HideTip(screen);
        foreach (var coin in s.Coins)
        {
            if (!GodotObject.IsInstanceValid(coin)) continue;
            coin.Disabled = !enabled;
            coin.FocusMode = enabled ? Control.FocusModeEnum.All : Control.FocusModeEnum.None;
            coin.MouseFilter = enabled ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
        }
    }

    // ───────────────────────────── Builders ─────────────────────────────

    private static T Track<T>(SkinState s, T node) where T : Node
    {
        s.Added.Add(node);
        return node;
    }

    private static Panel Box(string name, Color fill, Color border, int borderWidth)
    {
        var panel = new Panel { Name = name, MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", Style(fill, border, borderWidth, 0, 0));
        return panel;
    }

    private static void SetCorners(Panel panel, int topLeft, int topRight, int bottomRight, int bottomLeft)
    {
        if (panel.GetThemeStylebox("panel") is not StyleBoxFlat style) return;
        style.CornerRadiusTopLeft = topLeft;
        style.CornerRadiusTopRight = topRight;
        style.CornerRadiusBottomRight = bottomRight;
        style.CornerRadiusBottomLeft = bottomLeft;
    }

    private static StyleBoxFlat Style(Color fill, Color border, int borderWidth, float padX, float padY)
    {
        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            DrawCenter = fill.A > 0,
            ContentMarginLeft = padX,
            ContentMarginRight = padX,
            ContentMarginTop = padY,
            ContentMarginBottom = padY,
        };
        style.SetBorderWidthAll(borderWidth);
        return style;
    }

    private static void SetCoinStyle(Button coin, bool active)
    {
        var style = new StyleBoxFlat { BgColor = CoinFill, BorderColor = active ? Colors.White : GoldText };
        style.SetBorderWidthAll(3);
        style.SetCornerRadiusAll((int)(CoinSize / 2));
        style.ShadowColor = active ? GoldText : Colors.Black;
        style.ShadowSize = active ? 4 : 2;
        foreach (var state in new[] { "normal", "hover", "pressed", "disabled", "hover_pressed" })
            coin.AddThemeStyleboxOverride(state, style);
    }

    private static Label SmallCaps(NCharacterSelectScreen screen, string text, int size)
    {
        var font = new FontVariation { BaseFont = screen.GetNode<Control>("InfoPanel/VBoxContainer/Relic/Name/RichTextLabel").GetThemeFont("normal_font"), SpacingGlyph = 3 };
        var label = new Label { Text = text, Uppercase = true, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontOverride("font", font);
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", GoldText);
        return label;
    }

    private static ColorRect Line(string name, bool top)
    {
        var line = new ColorRect { Name = name, Color = SubtleLine, MouseFilter = Control.MouseFilterEnum.Ignore };
        line.SetAnchorsPreset(top ? Control.LayoutPreset.TopWide : Control.LayoutPreset.BottomWide);
        line.OffsetLeft = -FramePad / 2;
        line.OffsetRight = FramePad / 2;
        if (top) line.OffsetBottom = 1;
        else line.OffsetTop = -1;
        return line;
    }

    private static MarginContainer Margin(int x, int y)
    {
        var margin = new MarginContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        margin.AddThemeConstantOverride("margin_left", x);
        margin.AddThemeConstantOverride("margin_right", x);
        margin.AddThemeConstantOverride("margin_top", y);
        margin.AddThemeConstantOverride("margin_bottom", y);
        return margin;
    }

    /// <summary>Anchors to the parent's full rect, inset by <paramref name="inset"/> (negative grows it).</summary>
    private static T FullRect<T>(T control, float inset) where T : Control
    {
        control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        control.OffsetLeft = inset;
        control.OffsetTop = inset;
        control.OffsetRight = -inset;
        control.OffsetBottom = -inset;
        return control;
    }

    private static Texture2D? LoadIcon(CharacterPlaystyle playstyle)
    {
        try
        {
            var path = playstyle.IconPath();
            return ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Playstyle icon for {playstyle.Key} failed to load: {e.Message}");
            return null;
        }
    }

    private static string Loc(string key) =>
        LocString.Exists("characters", key) ? new LocString("characters", key).GetFormattedText() : key;
}
