using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
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
/// Super Form on a player's body: a gold Super Saiyan-like aura (the game's power-up flames, kept alive and recolored,
/// see <see cref="SuperFormAura"/>) with yellow particles rising around them (scenes/fx/rising_particles.tscn, like
/// Lurking Intensity's) and rare blue electric sparks drawn in code (super_form_sparks.tscn) while they have Rings;
/// at 0 Rings, the aura and particles go away, the sparks turn red and they sweat now and then.
/// Also keeps the Ring health bar segment and the local player's music up to date. Visual only.
/// </summary>
public sealed class SuperFormFx
{
    private static string FxScene(string name) => Path.Join(MainFile.ResPath, "scenes", "fx", name + ".tscn");

    private const float PollInterval = 0.2f;

    private static readonly Color BlueSparks = new(0.45f, 0.75f, 1f);
    private static readonly Color RedSparks = new(1f, 0.25f, 0.2f);
    private static readonly Color ParticleColor = new(1f, 0.85f, 0.15f);

    private readonly NCreature _node;
    private readonly Creature _creature;
    private readonly Rect2 _bounds;
    private readonly SuperFormAura _aura;
    private Node2D? _particles;
    private Node2D? _sparks;
    private Node2D? _sweat;

    private (bool Active, int Rings)? _last;

    private SuperFormFx(NCreature node)
    {
        _node = node;
        _creature = node.Entity;
        _bounds = new Rect2(node.Visuals.Bounds.Position, node.Visuals.Bounds.Size);
        _aura = new SuperFormAura(node);
    }

    public static void Attach(NCreature node)
    {
        if (node.Entity is not { IsPlayer: true }) return;
        var fx = new SuperFormFx(node);
        var timer = new Godot.Timer { WaitTime = PollInterval, Autostart = true };
        timer.Timeout += fx.Poll;
        node.AddChild(timer);
        node.TreeExiting += () => fx._aura.SetActive(false, instant: true);
        fx.Poll();
    }

    private void Poll()
    {
        if (!GodotObject.IsInstanceValid(_node) || _creature.Player is not { } player) return;
        var active = _creature.IsAlive && _creature.HasPower<SuperFormPower>();
        var rings = player.Gold;
        var hasRings = rings > 0;

        // Every poll: the game refuses to spawn the flames while the creature isn't interactable, so it retries
        _aura.SetActive(active && hasRings);

        var state = (active, rings);
        if (_last == state) return;
        var wasActive = _last?.Active ?? false;
        var hadRings = (_last?.Rings ?? 0) > 0;
        _last = state;

        SetParticles(active && hasRings);
        SetSparks(active, hasRings);
        SetSweat(active && !hasRings);
        RingVisuals.RefreshHealthBar(_node);

        if (LocalContext.IsMe(player) && (wasActive != active || hadRings != hasRings)) StrifeMusic.Refresh();
    }

    /// <summary>Yellow particles rising around the body, like Lurking Intensity's red ones.</summary>
    private void SetParticles(bool on)
    {
        if (_particles == null)
        {
            if (!on || !GodotObject.IsInstanceValid(_node.Visuals)) return;
            _particles = GD.Load<PackedScene>(FxScene("rising_particles")).Instantiate<Node2D>();
            _node.Visuals.AddChild(_particles);
            _particles.Call("set_color", ParticleColor);
            _particles.Call("set_area", _bounds);
        }
        if (GodotObject.IsInstanceValid(_particles)) _particles.Call("set_active", on);
    }

    /// <summary>Electric sparks drawn in code (scenes/fx/super_form_sparks.tscn).</summary>
    private void SetSparks(bool on, bool blue)
    {
        if (_sparks == null)
        {
            if (!on || !GodotObject.IsInstanceValid(_node.Visuals)) return;
            _sparks = GD.Load<PackedScene>(FxScene("super_form_sparks")).Instantiate<Node2D>();
            _node.Visuals.AddChild(_sparks);
            _sparks.Call("set_area", _bounds);
        }
        if (GodotObject.IsInstanceValid(_sparks)) _sparks.Call("set_sparks", on, blue ? BlueSparks : RedSparks);
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

/// <summary>
/// Super Form's steady aura, like Demon Form's: the game's power-up VFX (NPowerUpVfx, the flame columns behind the body
/// and the sparkles / beams in front) spawned once and kept alive. Its own script fades and frees it after 1 s, so its
/// processing is turned off; we fade it in / out ourselves and keep it on the creature if they move.
/// The 3D flames get gold colors on a copy of their material; the front sparkles and beams are already yellow.
/// Visual only. Fragile on purpose: it depends on how that base game scene is built (vfx_power_up_3d_back.tscn).
/// </summary>
public sealed class SuperFormAura(NCreature node)
{
    private const float FadeTime = 0.3f;

    // The base game's flames are Color(1, 0.28, 0) / Color(1, 0.71, 0)
    private static readonly Color FlameColor = new(1f, 0.68f, 0.05f);
    private static readonly Color FlameCoreColor = new(1f, 0.93f, 0.45f);

    private static readonly AccessTools.FieldRef<NPowerUpVfx, Sprite2D> BackVfxRef =
        AccessTools.FieldRefAccess<NPowerUpVfx, Sprite2D>("_backVfx");

    private NPowerUpVfx? _vfx;
    private Sprite2D? _back;
    private Vector2 _backOffset;
    private Tween? _tween;
    private bool _following;
    private bool _on;

    public void SetActive(bool on, bool instant = false)
    {
        if (on && !IsAlive()) _on = false;
        if (on == _on) return;
        if (on && !Spawn()) return;
        _on = on;

        if (on) Fade(1f, null);
        else if (instant) Free();
        else Fade(0f, Free);
    }

    private bool IsAlive() => _vfx != null && GodotObject.IsInstanceValid(_vfx) && _back != null && GodotObject.IsInstanceValid(_back);

    private bool Spawn()
    {
        if (IsAlive()) return true;
        Free();
        if (!GodotObject.IsInstanceValid(node)) return false;
        try
        {
            if (NPowerUpVfx.CreateNormal(node.Entity) is not { } vfx) return false;
            // _Ready already ran (it's added right away) and moved its back sprite to the back VFX layer
            if (BackVfxRef(vfx) is not { } back || !GodotObject.IsInstanceValid(back))
            {
                vfx.QueueFree();
                return false;
            }
            vfx.SetProcess(false);
            _vfx = vfx;
            _back = back;
            _backOffset = back.GlobalPosition - vfx.GlobalPosition;
            SetAlpha(0f);
            Recolor(vfx);

            if (!_following)
            {
                _following = true;
                node.GetTree().ProcessFrame += Follow;
            }
            return true;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Super Form aura: {ex}");
            Free();
            return false;
        }
    }

    /// <summary>Gold flames, on a copy of the material (the base game's is shared with every power-up).</summary>
    private static void Recolor(Node root)
    {
        if (root is MeshInstance3D { MaterialOverride: ShaderMaterial material } mesh)
        {
            var gold = (ShaderMaterial)material.Duplicate();
            gold.SetShaderParameter("ColorParameter", FlameColor);
            gold.SetShaderParameter("ColorParameter2", FlameCoreColor);
            mesh.MaterialOverride = gold;
        }
        foreach (var child in root.GetChildren()) Recolor(child);
    }

    /// <summary>Stays on the creature (Frontliner, Full Nelson...).</summary>
    private void Follow()
    {
        if (!IsAlive() || !GodotObject.IsInstanceValid(node))
        {
            StopFollowing();
            return;
        }
        var position = node.VfxSpawnPosition;
        if (_vfx!.GlobalPosition == position) return;
        _vfx.GlobalPosition = position;
        _back!.GlobalPosition = position + _backOffset;
    }

    private void StopFollowing()
    {
        if (!_following) return;
        _following = false;
        if (GodotObject.IsInstanceValid(node) && node.IsInsideTree()) node.GetTree().ProcessFrame -= Follow;
    }

    private void Fade(float alpha, Action? then)
    {
        _tween?.Kill();
        if (!IsAlive()) return;
        _tween = _vfx!.CreateTween();
        _tween.TweenMethod(Callable.From<float>(SetAlpha), _vfx.Modulate.A, alpha, FadeTime);
        if (then != null) _tween.TweenCallback(Callable.From(then));
    }

    private void SetAlpha(float alpha)
    {
        if (!IsAlive()) return;
        _vfx!.Modulate = new Color(1f, 1f, 1f, alpha);
        _back!.Modulate = new Color(1f, 1f, 1f, alpha);
    }

    private void Free()
    {
        _tween?.Kill();
        _tween = null;
        StopFollowing();
        if (_vfx != null && GodotObject.IsInstanceValid(_vfx)) _vfx.QueueFree();
        if (_back != null && GodotObject.IsInstanceValid(_back)) _back.QueueFree();
        _vfx = null;
        _back = null;
        _on = false;
    }
}
