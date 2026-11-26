using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// The Emerald wheel (Super Form, like Sonic Superstars): with <see cref="SuperForm"/>, the Chaos Emeralds don't add their
/// Emerald Powers to the Draw Pile. A button above the energy counter opens a wheel instead: each held Emerald's power can
/// be added to your hand once per fight (an enemy's death makes one used power available again, see
/// <see cref="SuperFormPower"/>). The top section turns the Rings' damage protection off and on.
/// </summary>
public static class EmeraldWheel
{
    /// <summary>Wheel sections clockwise from the top. Null = the Super Form toggle.</summary>
    public static readonly IReadOnlyList<ChaosEmeraldColor?> Sections =
    [
        null,
        ChaosEmeraldColor.Cyan,
        ChaosEmeraldColor.Purple,
        ChaosEmeraldColor.Green,
        ChaosEmeraldColor.Blue,
        ChaosEmeraldColor.Yellow,
        ChaosEmeraldColor.Red,
        ChaosEmeraldColor.Grey,
    ];

    /// <summary>Choice sent for the Super Form toggle (colors are sent as their enum value).</summary>
    public const int ToggleChoice = 15;

    public static bool CanPick(Player player, ChaosEmeraldColor color) =>
        player.Creature.GetPower<SuperFormPower>() is { } power && !power.IsUsed(color) &&
        ChaosEmeraldHelper.HeldColors(player).Contains(color);

    /// <summary>Asks for the choice through the action queue: it runs at the same point on every client.</summary>
    public static void Request(Player player, ChaosEmeraldColor? color) =>
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new EmeraldWheelAction(player, color is { } c ? (int)c : ToggleChoice));
}

/// <summary>A pick on the Emerald wheel (see <see cref="EmeraldWheel"/>). Checked again when it runs.</summary>
public sealed class EmeraldWheelAction(Player player, int choice) : GameAction
{
    public override ulong OwnerId => player.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    protected override async Task ExecuteAction()
    {
        if (!CombatManager.Instance.IsInProgress || player.Creature.IsDead) return;
        if (player.Creature.GetPower<SuperFormPower>() is not { } power) return;

        if (choice == EmeraldWheel.ToggleChoice)
        {
            power.ToggleProtection();
            return;
        }

        var color = (ChaosEmeraldColor)choice;
        if (!Enum.IsDefined(color) || !EmeraldWheel.CanPick(player, color)) return;
        if (player.Creature.CombatState is not { } combatState) return;

        power.MarkUsed(color);
        var card = combatState.CreateCard(ChaosEmeraldHelper.EmeraldPowerCard(color), player);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }

    public override INetAction ToNetAction() => new NetEmeraldWheelAction { choice = choice };

    public override string ToString() => $"{nameof(EmeraldWheelAction)} for player {player.NetId} choice {choice}";
}

/// <summary>Network form of <see cref="EmeraldWheelAction"/> (mod INetActions are registered by the game itself).</summary>
public struct NetEmeraldWheelAction : INetAction
{
    public int choice;

    public GameAction ToGameAction(Player player) => new EmeraldWheelAction(player, choice);

    public void Serialize(PacketWriter writer) => writer.WriteUInt((uint)choice, 4);

    public void Deserialize(PacketReader reader) => choice = (int)reader.ReadUInt(4);

    public override string ToString() => $"{nameof(NetEmeraldWheelAction)} choice: {choice}";
}

/// <summary>
/// The Emerald wheel button (the player's Chaos Emeralds relic) above the draw pile button, shown while the local player is
/// in Super Form, and the wheel it opens (scenes/fx/emerald_wheel.tscn). Hovering a section shows its card (or Super Form).
/// Controller: the button can be focused and selected; the wheel follows the left stick / D-pad. Visual only: the pick
/// itself goes through <see cref="EmeraldWheelAction"/>.
/// </summary>
[HarmonyPatch(typeof(NCombatCardPile), nameof(NCombatCardPile.Initialize))]
public static class EmeraldWheelUi
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "emerald_wheel.tscn");

    private const string ButtonName = "EmeraldWheelButton";

    // A card tip and text tips are laid out from the same point unless aligned: the text ended up behind the card
    private static readonly AccessTools.FieldRef<NHoverTipSet, VFlowContainer> TextTipsRef =
        AccessTools.FieldRefAccess<NHoverTipSet, VFlowContainer>("_textHoverTipContainer");
    private static readonly AccessTools.FieldRef<NHoverTipSet, NHoverTipCardContainer> CardTipsRef =
        AccessTools.FieldRefAccess<NHoverTipSet, NHoverTipCardContainer>("_cardHoverTipContainer");

    private const float ButtonSize = 72f;
    private const float VerticalGap = 10f;
    private const float PollInterval = 0.1f;
    private const float WheelOuterRadius = 290f;

    private static string IconPath(string name) => Path.Join(MainFile.ResPath, "images", "ui", "emerald_wheel", name + ".png");

    private static string IconName(ChaosEmeraldColor color) => color switch
    {
        ChaosEmeraldColor.Purple => "vision",
        ChaosEmeraldColor.Cyan => "water",
        ChaosEmeraldColor.Green => "ivy",
        ChaosEmeraldColor.Yellow => "slow",
        ChaosEmeraldColor.Blue => "avatar",
        ChaosEmeraldColor.Red => "bullet",
        _ => "extra",
    };

    private static Texture2D? Load(string path) => ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;

    /// <summary>Puts the text tips (already used / missing) right of the card tip instead of behind it.</summary>
    private static void PlaceTextRightOfCard(NHoverTipSet set)
    {
        if (!GodotObject.IsInstanceValid(set)) return;
        var text = TextTipsRef(set);
        var cards = CardTipsRef(set);
        if (text == null || cards == null || cards.GetChildCount() == 0) return;
        var cardRect = cards.GetGlobalRect();
        text.GlobalPosition = new Vector2(cardRect.End.X + 10f, cardRect.Position.Y);
    }

    [HarmonyPostfix]
    public static void Postfix(NCombatCardPile __instance, Player player)
    {
        if (__instance is not NDrawPileButton || !ResourceLoader.Exists(ScenePath)) return;
        if (__instance.GetParent() is not { } parent) return;
        // Initialize runs again for later fights / runs: one button, for this player
        if (parent.GetNodeOrNull(ButtonName) is { } old) old.QueueFree();
        var pile = (Control)__instance;

        var relic = (ChaosEmeraldRelic)ChaosEmeraldHelper.ComboRelic(ChaosEmeraldHelper.ColorFor(player));
        var button = new TextureButton
        {
            Name = ButtonName,
            TextureNormal = Load(relic.PackedIconPath),
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(ButtonSize, ButtonSize),
            Size = new Vector2(ButtonSize, ButtonSize),
            FocusMode = Control.FocusModeEnum.All,
            Visible = false,
        };
        // Sibling, not child: the pile button scales up on hover and slides in and out. Follows it every frame
        button.TopLevel = true;
        parent.AddChild(button);
        var tree = pile.GetTree();
        void Follow()
        {
            if (!GodotObject.IsInstanceValid(button) || !GodotObject.IsInstanceValid(pile)) return;
            var rect = pile.GetGlobalRect();
            button.GlobalPosition = new Vector2(rect.Position.X + (rect.Size.X - ButtonSize) * 0.5f, rect.Position.Y - ButtonSize - VerticalGap);
        }
        tree.ProcessFrame += Follow;
        button.TreeExiting += () => tree.ProcessFrame -= Follow;

        Control? wheel = null;
        string? shownState = null;

        bool CanUse() =>
            CombatManager.Instance.IsInProgress && player.Creature.IsAlive && player.Creature.HasPower<SuperFormPower>();

        string State()
        {
            var power = player.Creature.GetPower<SuperFormPower>();
            return string.Join(",", EmeraldWheel.Sections.Select(c =>
                c is { } color ? EmeraldWheel.CanPick(player, color).ToString() : (power?.ProtectionOn ?? false).ToString()));
        }

        void Fill()
        {
            if (wheel == null || !GodotObject.IsInstanceValid(wheel)) return;
            var power = player.Creature.GetPower<SuperFormPower>();
            var textures = new Godot.Collections.Array();
            var selectable = new Godot.Collections.Array();
            foreach (var section in EmeraldWheel.Sections)
            {
                if (section is { } color)
                {
                    var available = EmeraldWheel.CanPick(player, color);
                    textures.Add(Load(IconPath(IconName(color) + (available ? "_available" : "_used"))));
                    selectable.Add(available);
                }
                else
                {
                    textures.Add(Load(IconPath(power?.ProtectionOn == false ? "super_form_off" : "super_form_on")));
                    selectable.Add(true);
                }
            }
            wheel.Call("setup", textures, selectable);
            shownState = State();
        }

        void Close()
        {
            if (wheel == null) return;
            if (GodotObject.IsInstanceValid(wheel))
            {
                NHoverTipSet.Remove(wheel);
                wheel.QueueFree();
            }
            wheel = null;
            if (GodotObject.IsInstanceValid(button) && button.Visible) button.GrabFocus();
        }

        void ShowTip(int index)
        {
            if (wheel == null || !GodotObject.IsInstanceValid(wheel)) return;
            NHoverTipSet.Remove(wheel);
            if (index < 0 || index >= EmeraldWheel.Sections.Count) return;

            var tips = new List<IHoverTip>();
            if (EmeraldWheel.Sections[index] is { } color)
            {
                var held = ChaosEmeraldHelper.HeldColors(player).Contains(color);
                // On cooldown: only the "already used" text, the card preview hid it
                if (!held || EmeraldWheel.CanPick(player, color))
                    tips.Add(HoverTipFactory.FromCard(ChaosEmeraldHelper.EmeraldPowerCard(color)));
                if (!EmeraldWheel.CanPick(player, color))
                {
                    tips.Add(new HoverTip(new LocString("static_hover_tips",
                        held ? "SHIONTHEBRUISER-EMERALD_WHEEL_USED.description" : "SHIONTHEBRUISER-EMERALD_WHEEL_MISSING.description")));
                }
            }
            else
            {
                var on = player.Creature.GetPower<SuperFormPower>()?.ProtectionOn ?? true;
                tips.Add(new HoverTip(new LocString("static_hover_tips", "SHIONTHEBRUISER-EMERALD_WHEEL_TOGGLE.title"),
                    new LocString("static_hover_tips",
                        on ? "SHIONTHEBRUISER-EMERALD_WHEEL_TOGGLE.on" : "SHIONTHEBRUISER-EMERALD_WHEEL_TOGGLE.off")));
            }

            var center = wheel.GetGlobalRect().GetCenter();
            if (NHoverTipSet.CreateAndShow(wheel, tips) is not { } set) return;
            set.SetGlobalPosition(center + new Vector2(WheelOuterRadius + 30f, -200f));
            if (tips.Count < 2 || tips[0] is not CardHoverTip) return;
            PlaceTextRightOfCard(set);
            // Sizes settle after layout: once more next frame
            Callable.From(() => PlaceTextRightOfCard(set)).CallDeferred();
        }

        void Open()
        {
            if (wheel != null || !CanUse() || NRun.Instance?.GlobalUi is not { } globalUi) return;
            NHoverTipSet.Remove(button);
            wheel = GD.Load<PackedScene>(ScenePath).Instantiate<Control>();
            globalUi.AddChild(wheel);
            wheel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            wheel.Connect("hovered", Callable.From<int>(ShowTip));
            wheel.Connect("chosen", Callable.From<int>(index =>
            {
                // A second pick before the wheel is freed (double click) would send a second action: a toggle would undo itself
                if (wheel == null) return;
                if (index >= 0 && index < EmeraldWheel.Sections.Count && CanUse())
                {
                    EmeraldWheel.Request(player, EmeraldWheel.Sections[index]);
                }
                Close();
            }));
            wheel.Connect("closed", Callable.From(Close));
            Fill();
        }

        button.Pressed += Open;
        button.GuiInput += inputEvent =>
        {
            // The game's controller "select" isn't Godot's ui_accept
            if (!inputEvent.IsActionPressed("ui_select")) return;
            Open();
            button.AcceptEvent();
        };
        button.MouseEntered += () =>
        {
            NHoverTipSet.CreateAndShow(button, new HoverTip(
                    new LocString("static_hover_tips", "SHIONTHEBRUISER-EMERALD_WHEEL.title"),
                    new LocString("static_hover_tips", "SHIONTHEBRUISER-EMERALD_WHEEL.description")))
                ?.SetGlobalPosition(button.GlobalPosition + new Vector2(ButtonSize + 12f, -60f));
        };
        button.MouseExited += () => NHoverTipSet.Remove(button);

        void Refresh()
        {
            if (!GodotObject.IsInstanceValid(button)) return;
            var visible = CanUse();
            if (button.Visible != visible)
            {
                button.Visible = visible;
                if (!visible) NHoverTipSet.Remove(button);
            }
            if (!visible)
            {
                Close();
                return;
            }
            // Dimmed once nothing is left to pick (the toggle stays usable)
            var anyLeft = ChaosEmeraldHelper.AllColors.Any(c => EmeraldWheel.CanPick(player, c));
            button.SelfModulate = anyLeft ? Colors.White : new Color(0.55f, 0.55f, 0.55f);
            if (wheel != null && shownState != State()) Fill();
        }

        var timer = new Godot.Timer { WaitTime = PollInterval, Autostart = true };
        timer.Timeout += Refresh;
        button.AddChild(timer);
        button.TreeExiting += Close;
        Refresh();
    }
}
