using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.sts2.Core.Nodes.TopBar;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Super Form's Rings, cosmetic only:
/// <list type="bullet">
/// <item>The top bar's Gold icon becomes a Ring while the local player has the Super Form relic.</item>
/// <item>In a fight with Super Form, a yellow segment from the left of the health bar shows how much damage the Rings
/// can still absorb (full when you have at least your Max HP in Rings).</item>
/// </list>
/// </summary>
public static class RingVisuals
{
    public static readonly string RingIconPath = "ui/ring.png".ImagePath();

    private static Texture2D? _originalGoldIcon;

    /// <summary>Swaps the top bar's Gold icon for the Ring (or back).</summary>
    public static void RefreshGoldIcon()
    {
        var gold = NRun.Instance?.GlobalUi?.TopBar?.Gold;
        var state = StrifeRun.State;
        if (gold == null || !GodotObject.IsInstanceValid(gold) || state == null) return;
        if (FindIcon(gold) is not { } icon) return;

        var useRing = SuperForm.Has(LocalContext.GetMe(state.Players)) && ResourceLoader.Exists(RingIconPath);
        if (useRing)
        {
            if (icon.Texture?.ResourcePath != RingIconPath)
            {
                _originalGoldIcon ??= icon.Texture;
                icon.Texture = GD.Load<Texture2D>(RingIconPath);
            }
        }
        else if (_originalGoldIcon != null && icon.Texture?.ResourcePath == RingIconPath)
        {
            icon.Texture = _originalGoldIcon;
        }
    }

    private static TextureRect? FindIcon(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is TextureRect rect) return rect;
            if (FindIcon(child) is { } found) return found;
        }
        return null;
    }

    public static void RefreshHealthBar(NCreature node)
    {
        if (FindHealthBar(node) is { } bar) bar.RefreshValues();
    }

    private static NHealthBar? FindHealthBar(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is NHealthBar bar) return bar;
            if (FindHealthBar(child) is { } found) return found;
        }
        return null;
    }
}

[HarmonyPatch(typeof(NTopBarGold), nameof(NTopBarGold.Initialize))]
public static class RingTopBarPatch
{
    [HarmonyPostfix]
    public static void Postfix(Player player) => Callable.From(RingVisuals.RefreshGoldIcon).CallDeferred();
}

/// <summary>The yellow Ring segment on a Super Form player's health bar.</summary>
[HarmonyPatch(typeof(NHealthBar), nameof(NHealthBar.RefreshValues))]
public static class RingHealthBarPatch
{
    private const string SegmentName = "StrifeRingsForeground";
    private const float MinWidth = 12f;
    private static readonly Color SegmentColor = new(1f, 0.85f, 0.1f, 0.9f);

    private static readonly AccessTools.FieldRef<NHealthBar, Creature> CreatureRef =
        AccessTools.FieldRefAccess<NHealthBar, Creature>("_creature");
    private static readonly AccessTools.FieldRef<NHealthBar, Control> PoisonForegroundRef =
        AccessTools.FieldRefAccess<NHealthBar, Control>("_poisonForeground");
    private static readonly Func<NHealthBar, float> MaxFgWidth =
        AccessTools.MethodDelegate<Func<NHealthBar, float>>(AccessTools.PropertyGetter(typeof(NHealthBar), "MaxFgWidth"));

    [HarmonyPostfix]
    public static void Postfix(NHealthBar __instance)
    {
        var creature = CreatureRef(__instance);
        var poison = PoisonForegroundRef(__instance);
        if (creature is null || poison is null || !GodotObject.IsInstanceValid(poison)) return;
        if (poison.GetParent() is not { } mask) return;

        var segment = mask.GetNodeOrNull<NinePatchRect>(SegmentName);
        var rings = creature.IsAlive && creature.HasPower<SuperFormPower>() ? creature.Player?.Gold ?? 0 : 0;
        if (rings <= 0 || creature.MaxHp <= 0)
        {
            if (segment != null) segment.Visible = false;
            return;
        }

        if (segment == null)
        {
            segment = (NinePatchRect)poison.Duplicate((int)(Node.DuplicateFlags.Groups | Node.DuplicateFlags.UseInstantiation));
            segment.Name = SegmentName;
            segment.UniqueNameInOwner = false;
            segment.SelfModulate = SegmentColor;
            segment.Modulate = Colors.White;
            segment.MouseFilter = Control.MouseFilterEnum.Ignore;
            segment.AnchorLeft = 0f;
            segment.AnchorRight = 1f;
            // Drawn last: on top of the HP segment
            mask.AddChild(segment);
        }

        var width = MaxFgWidth(__instance);
        var end = Math.Max(Math.Min((float)rings / creature.MaxHp, 1f) * width, MinWidth);
        segment.Visible = true;
        segment.OffsetLeft = 0f;
        segment.OffsetRight = end - width;
    }
}

/// <summary>
/// Super Form on a player's body: a yellow, Super Saiyan-like aura drawn in code that lights the body
/// (scenes/fx/super_form_aura.tscn), with rare blue sparks
/// while they have Rings; at 0 Rings, the aura goes away, the sparks turn red and they sweat now and then.
/// Also keeps the Ring health bar segment and the local player's music up to date. Visual only.
/// </summary>
public sealed class SuperFormFx
{
    private static string FxScene(string name) => Path.Join(MainFile.ResPath, "scenes", "fx", name + ".tscn");

    private const float PollInterval = 0.2f;

    private static readonly Color BlueSparks = new(0.35f, 0.65f, 1.6f);
    private static readonly Color RedSparks = new(1.6f, 0.25f, 0.2f);

    private readonly NCreature _node;
    private readonly Creature _creature;
    private readonly Rect2 _bounds;
    private Node2D? _aura;
    private Node2D? _sparks;
    private Node2D? _sweat;

    private (bool Active, int Rings)? _last;

    private SuperFormFx(NCreature node)
    {
        _node = node;
        _creature = node.Entity;
        _bounds = new Rect2(node.Visuals.Bounds.Position, node.Visuals.Bounds.Size);
    }

    public static void Attach(NCreature node)
    {
        if (node.Entity is not { IsPlayer: true }) return;
        var fx = new SuperFormFx(node);
        var timer = new Godot.Timer { WaitTime = PollInterval, Autostart = true };
        timer.Timeout += fx.Poll;
        node.AddChild(timer);
        fx.Poll();
    }

    private void Poll()
    {
        if (!GodotObject.IsInstanceValid(_node) || _creature.Player is not { } player) return;
        var active = _creature.IsAlive && _creature.HasPower<SuperFormPower>();
        var rings = player.Gold;
        var state = (active, rings);
        if (_last == state) return;
        var wasActive = _last?.Active ?? false;
        var hadRings = (_last?.Rings ?? 0) > 0;
        _last = state;

        var hasRings = rings > 0;
        SetAura(active && hasRings);
        SetSparks(active, hasRings);
        SetSweat(active && !hasRings);
        RingVisuals.RefreshHealthBar(_node);

        if (LocalContext.IsMe(player) && (wasActive != active || hadRings != hasRings)) StrifeMusic.Refresh();
    }

    private void SetAura(bool on)
    {
        if (_aura == null)
        {
            if (!on) return;
            _aura = GD.Load<PackedScene>(FxScene("super_form_aura")).Instantiate<Node2D>();
            _node.Visuals.AddChild(_aura);
            // Behind the body
            _node.Visuals.MoveChild(_aura, 0);
            _aura.Call("set_area", _bounds);
            // The aura's light only lights bodies on this layer (players already are, see CreatureFxController)
            if (_node.Visuals.GetCurrentBody() is CanvasItem body) body.LightMask |= CreatureFxController.AuraLightLayer;
        }
        if (GodotObject.IsInstanceValid(_aura)) _aura.Call("set_active", on);
    }

    private void SetSparks(bool on, bool blue)
    {
        if (_sparks == null)
        {
            if (!on) return;
            _sparks = GD.Load<PackedScene>(FxScene("berserk_sparks")).Instantiate<Node2D>();
            // Rarer than Berserk's
            _sparks.Set("min_interval", 1.2f);
            _sparks.Set("max_interval", 3.0f);
            _sparks.Set("double_spark_chance", 0.15f);
            _node.Visuals.AddChild(_sparks);
            _sparks.Call("set_area", _bounds.Grow(20f));
        }
        if (!GodotObject.IsInstanceValid(_sparks)) return;
        _sparks.Modulate = blue ? BlueSparks : RedSparks;
        _sparks.Call("set_active", on);
    }

    private void SetSweat(bool on)
    {
        if (_sweat == null)
        {
            if (!on) return;
            _sweat = GD.Load<PackedScene>(FxScene("tears")).Instantiate<Node2D>();
            _node.Visuals.AddChild(_sweat);
            var head = ShionVfx.HeadTop(_bounds, true);
            var y = _bounds.Position.Y + _bounds.Size.Y * 0.18f;
            var gap = _bounds.Size.X * 0.14f;
            _sweat.Call("set_head", new Vector2(head.X - gap, y), new Vector2(head.X + gap, y));
            _sweat.Call("set_stacks", 1);
        }
        if (GodotObject.IsInstanceValid(_sweat)) _sweat.Call("set_active", on);
    }
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class SuperFormFxPatch
{
    [HarmonyPostfix]
    public static void Postfix(NCreature __instance)
    {
        if (__instance.Entity is not { IsPlayer: true } || __instance.Visuals == null) return;
        if (__instance.Visuals.IsNodeReady())
        {
            SuperFormFx.Attach(__instance);
            return;
        }
        __instance.Visuals.Connect(Node.SignalName.Ready, Callable.From(() => SuperFormFx.Attach(__instance)),
            (uint)GodotObject.ConnectFlags.OneShot);
    }
}
