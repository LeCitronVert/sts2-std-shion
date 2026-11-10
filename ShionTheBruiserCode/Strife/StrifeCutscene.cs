using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// The Chaos Emeralds cutscene at the end of Act 3, on top of the Architect's room. Visual only: the relics are given by
/// <see cref="StrifeTransition"/>, the same on every client; each client only animates its own player.
/// <list type="number">
/// <item>Everything but the players fades to black; the players lie "dead", with their health bars and relics.</item>
/// <item>The seven Emeralds come out of the local player and spin around them, pulsing one by one.</item>
/// <item>They converge back into the player, and the screen fades gently to white, staying white until Act 4 starts.</item>
/// </list>
/// </summary>
public sealed class StrifeCutscene
{
    private const float FadeTime = 1.2f;
    private const float EmergeTime = 0.9f;
    private const float ConvergeTime = 0.9f;
    private const float SpinPeriod = 2.4f;
    private const float GemSize = 72f;

    private readonly NCombatRoom _room;
    private readonly NCreature? _player;
    private readonly Node2D? _orbit;
    private readonly Dictionary<ChaosEmeraldColor, Sprite2D> _gems = new();
    private float _radius;
    private float _angle;
    private float _maxRadius;
    private ColorRect? _white;

    private StrifeCutscene(NCombatRoom room, NCreature? player)
    {
        _room = room;
        _player = player;
        if (player == null) return;

        var bounds = player.Visuals.Bounds;
        _orbit = new Node2D { Name = "StrifeEmeraldOrbit", Position = bounds.Position + bounds.Size * 0.5f, ZIndex = 5 };
        player.Visuals.AddChild(_orbit);
        _maxRadius = Math.Max(bounds.Size.X * 0.75f, 150f);
    }

    public static StrifeCutscene? Start(RunState state)
    {
        if (TestMode.IsOn || NCombatRoom.Instance is not { } room) return null;
        var me = LocalContext.GetMe(state.Players);
        return new StrifeCutscene(room, me == null ? null : room.GetCreatureNode(me.Creature));
    }

    public async Task FadeToBlack()
    {
        HideSpeechBubbles();
        var tween = _room.CreateTween().SetParallel();
        // Always one tweener, so the tween runs (and finishes) even if there's nothing else to fade
        tween.TweenInterval(FadeTime);
        if (_room.Background != null && GodotObject.IsInstanceValid(_room.Background))
        {
            tween.TweenProperty(_room.Background, "modulate", Colors.Black, FadeTime);
        }
        foreach (var node in _room.CreatureNodes.Where(n => n.Entity?.Side == CombatSide.Enemy))
        {
            tween.TweenProperty(node, "modulate:a", 0f, FadeTime);
        }
        foreach (var node in _room.CreatureNodes.Where(n => n.Entity is { IsPlayer: true }))
        {
            await CreatureCmd.TriggerAnim(node.Entity, "Dead", 0f);
        }
        await _room.ToSignal(tween, Tween.SignalName.Finished);
    }

    public void EmergeEmeralds()
    {
        if (_orbit == null) return;
        foreach (var color in ChaosEmeraldHelper.AllColors)
        {
            var icon = ChaosEmeraldHelper.SoloRelic(color).Icon;
            var sprite = new Sprite2D { Texture = icon, Scale = Vector2.Zero };
            _orbit.AddChild(sprite);
            _gems[color] = sprite;
        }

        var target = Vector2.One * GemScale();
        var tween = _orbit.CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(r => { _radius = r; Place(); }), 0f, _maxRadius, EmergeTime)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        foreach (var sprite in _gems.Values) tween.TweenProperty(sprite, "scale", target, EmergeTime);

        var spin = _orbit.CreateTween().SetLoops();
        spin.TweenMethod(Callable.From<float>(a => { _angle = a; Place(); }), 0f, Mathf.Tau, SpinPeriod);
    }

    private float GemScale()
    {
        var texture = _gems.Values.FirstOrDefault()?.Texture;
        return texture == null ? 1f : GemSize / Math.Max(texture.GetWidth(), texture.GetHeight());
    }

    private void Place()
    {
        var i = 0;
        foreach (var sprite in _gems.Values)
        {
            if (!GodotObject.IsInstanceValid(sprite)) continue;
            var a = _angle + Mathf.Tau * i / _gems.Count;
            // Slightly flattened orbit, so it reads as circling around the body
            sprite.Position = new Vector2(Mathf.Cos(a) * _radius, Mathf.Sin(a) * _radius * 0.55f);
            sprite.ZIndex = Mathf.Sin(a) >= 0 ? 1 : -1;
            i++;
        }
    }

    public void Pulse(ChaosEmeraldColor color)
    {
        if (!_gems.TryGetValue(color, out var sprite) || !GodotObject.IsInstanceValid(sprite)) return;
        var normal = Vector2.One * GemScale();
        var tween = sprite.CreateTween();
        tween.TweenProperty(sprite, "scale", normal * 1.6f, 0.12f).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(sprite, "scale", normal, 0.3f).SetEase(Tween.EaseType.In);
        tween.Parallel().TweenProperty(sprite, "modulate", new Color(2f, 2f, 2f), 0.12f);
        tween.TweenProperty(sprite, "modulate", Colors.White, 0.3f);
    }

    public async Task Converge()
    {
        if (_orbit == null || !GodotObject.IsInstanceValid(_orbit)) return;
        var tween = _orbit.CreateTween().SetParallel();
        tween.TweenMethod(Callable.From<float>(r => { _radius = r; Place(); }), _radius, 0f, ConvergeTime)
            .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Cubic);
        foreach (var sprite in _gems.Values) tween.TweenProperty(sprite, "scale", Vector2.Zero, ConvergeTime);
        await _orbit.ToSignal(tween, Tween.SignalName.Finished);
        _orbit.QueueFree();
    }

    /// <summary>The screen fades gently to white, and stays white until <see cref="FadeOutWhite"/> (after Act 4 is entered).</summary>
    public void Flash()
    {
        _white = new ColorRect { Color = new Color(1f, 1f, 1f, 0f), MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 100 };
        _white.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        ShionVfx.AddToScreen(_white);
        _white.CreateTween().TweenProperty(_white, "color:a", 1f, 0.6f).SetEase(Tween.EaseType.Out);
    }

    public void FadeOutWhite()
    {
        if (_white == null || !GodotObject.IsInstanceValid(_white)) return;
        var white = _white;
        _white = null;
        var tween = white.CreateTween();
        tween.TweenProperty(white, "color:a", 0f, 1.2f).SetEase(Tween.EaseType.In);
        tween.TweenCallback(Callable.From(white.QueueFree));
    }

    /// <summary>The Architect's last line (a speech bubble that never goes away on its own) leaves with the room.</summary>
    private void HideSpeechBubbles()
    {
        if (NGame.Instance is not { } game) return;
        foreach (var bubble in FindBubbles(game)) TaskHelper.RunSafely(bubble.AnimOut());
    }

    private static IEnumerable<NSpeechBubbleVfx> FindBubbles(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is NSpeechBubbleVfx bubble && GodotObject.IsInstanceValid(bubble)) yield return bubble;
            foreach (var nested in FindBubbles(child)) yield return nested;
        }
    }
}
