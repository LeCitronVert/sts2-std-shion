using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Moves a creature's visuals (body, VFX anchors) away from their place for attack animations, and back.
/// Only the visuals move: the creature's health bar, intents and hitbox stay where they are.
/// Visual only: the waits it adds go through <c>Cmd.CustomScaledWait</c>, like the game's own attack animations,
/// and every client runs them the same number of times.
/// <list type="bullet">
/// <item>Energy Volley: the target is pushed back a little on each hit, and comes back at the end.</item>
/// <item>Dragon Smash: the target is sent flying to the top of the screen, bounces around the screen edges
/// (one hit per collision), spinning, and the last hit slams it back in its place.</item>
/// <item>Pinball Move: Dragon Smash's flight on the attacker, which bounces off the screen edges and slams into
/// each hit's target.</item>
/// </list>
/// </summary>
public static class CreatureMotion
{
    private class State
    {
        public Tween? Tween;
        /// <summary>Spin angle, kept across Dragon Smash's segments.</summary>
        public float Angle;
        /// <summary>Screen edge of the current Dragon Smash collision (0 top, 1 right, 2 bottom, 3 left), or -1.</summary>
        public int Edge = -1;
    }

    private static readonly ConditionalWeakTable<NCreatureVisuals, State> States = new();

    // Energy Volley
    private const float KnockbackDistance = 22f;
    private const float MaxKnockback = 160f;

    // Dragon Smash
    private const float FastLaunchTime = 0.22f;
    private const float StandardLaunchTime = 0.38f;
    private const float FastBounceTime = 0.14f;
    private const float StandardBounceTime = 0.24f;
    /// <summary>Spin speed while flying, in turns per second.</summary>
    private const float SpinSpeed = 3.5f;
    /// <summary>How far inside the screen the creature's center stops when colliding with an edge.</summary>
    private const float EdgeMargin = 70f;
    private const string BluntVfx = "vfx/vfx_attack_blunt";

    private static NCreatureVisuals? VisualsOf(Creature creature)
    {
        var visuals = creature.GetCreatureNode()?.Visuals;
        return visuals != null && GodotObject.IsInstanceValid(visuals) ? visuals : null;
    }

    private static State StateOf(NCreatureVisuals visuals)
    {
        var state = States.GetOrCreateValue(visuals);
        if (state.Tween != null && state.Tween.IsValid()) state.Tween.Kill();
        state.Tween = null;
        return state;
    }

    /// <summary>Energy Volley: pushes the target back (away from the attacker) a little.</summary>
    public static void Knockback(Creature target)
    {
        if (VisualsOf(target) is not { } visuals) return;
        var state = StateOf(visuals);
        // Enemies are pushed right, players left
        var direction = target.IsPlayer ? -1f : 1f;
        var x = Mathf.Clamp(visuals.Position.X + direction * KnockbackDistance, -MaxKnockback, MaxKnockback);
        state.Tween = visuals.CreateTween();
        state.Tween.TweenProperty(visuals, "position", new Vector2(x, visuals.Position.Y), 0.08f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }

    /// <summary>Brings the creature back to its place, upright.</summary>
    public static void ReturnHome(Creature creature, float duration = 0.25f)
    {
        if (VisualsOf(creature) is not { } visuals) return;
        var state = StateOf(visuals);
        state.Edge = -1;
        if (visuals.Position == Vector2.Zero && Mathf.IsZeroApprox(visuals.Rotation)) return;
        var targetRotation = Mathf.Round(visuals.Rotation / Mathf.Tau) * Mathf.Tau;
        state.Tween = visuals.CreateTween().SetParallel();
        state.Tween.TweenProperty(visuals, "position", Vector2.Zero, duration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        state.Tween.TweenProperty(visuals, "rotation", targetRotation, duration);
        state.Tween.Chain().TweenCallback(Callable.From(() => visuals.Rotation = 0f));
        state.Angle = 0f;
    }

    // ---------------------------------------------------------------- Dragon Smash

    /// <summary>
    /// Dragon Smash's animation: Shion's attack animation plays once and sends the target flying (no damage),
    /// then each hit is a collision with a screen edge, the last one back in its place.
    /// Replaces the attack's hit VFX (played at each collision instead). Call <see cref="ReturnHome"/> after the attack,
    /// in case it was cut short.
    /// </summary>
    public static AttackCommand WithDragonSmash(this AttackCommand command, Creature target, int hits)
    {
        var hit = 0;
        return command
            .OnlyPlayAnimOnce()
            .AfterAttackerAnim(async () =>
            {
                if (!target.IsAlive) return;
                VfxCmd.PlayOnCreatureCenter(target, BluntVfx);
                var duration = EnergyBlast.ScaledTime(FastLaunchTime, StandardLaunchTime);
                if (duration > 0f) Fly(target, NextEdgePoint(target, launch: true), duration, landUpright: false);
                await Cmd.CustomScaledWait(FastLaunchTime, StandardLaunchTime);
            })
            .BeforeDamage(async () =>
            {
                hit++;
                if (!target.IsAlive) return;
                var last = hit >= hits;
                var duration = EnergyBlast.ScaledTime(FastBounceTime, StandardBounceTime);
                if (duration > 0f) Fly(target, last ? null : NextEdgePoint(target, launch: false), duration, landUpright: last);
                await Cmd.CustomScaledWait(FastBounceTime, StandardBounceTime);
                VfxCmd.PlayOnCreatureCenter(target, BluntVfx);
                NGame.Instance?.ScreenShake(last ? ShakeStrength.Medium : ShakeStrength.Weak, ShakeDuration.Short);
            });
    }

    // ---------------------------------------------------------------- Pinball Move

    /// <summary>How far from the enemy's center the attacker's center stops when it hits it.</summary>
    private const float PinballContactDistance = 80f;

    /// <summary>
    /// Pinball Move's animation: Dragon Smash's flight, but on the attacker. Its attack animation plays once and
    /// launches it towards the top of the screen, then for each hit it bounces off a screen edge (not before the first
    /// hit) and slams into that hit's (random) target. Replaces the attack's hit VFX (played on each impact instead).
    /// Call <see cref="ReturnHome"/> on the attacker after the attack.
    /// </summary>
    public static AttackCommand WithPinball(this AttackCommand command, Creature attacker)
    {
        Creature? target = null;
        var hit = 0;
        return command
            .OnlyPlayAnimOnce()
            .AfterAttackerAnim(async () =>
            {
                var duration = EnergyBlast.ScaledTime(FastLaunchTime, StandardLaunchTime);
                if (duration > 0f) Fly(attacker, NextEdgePoint(attacker, launch: true), duration, landUpright: false);
                await Cmd.CustomScaledWait(FastLaunchTime, StandardLaunchTime);
            })
            // Runs once the hit's random target is picked, before the damage: only remembers it (no node added).
            .WithHitVfxNode(creature =>
            {
                target = creature;
                return null;
            })
            .BeforeDamage(async () =>
            {
                hit++;
                if (target is null || !attacker.IsAlive) return;
                var duration = EnergyBlast.ScaledTime(FastBounceTime, StandardBounceTime);
                if (hit > 1)
                {
                    if (duration > 0f) Fly(attacker, NextEdgePoint(attacker, launch: false), duration, landUpright: false);
                    await Cmd.CustomScaledWait(FastBounceTime, StandardBounceTime);
                }
                if (duration > 0f) Fly(attacker, ContactPoint(attacker, target), duration, landUpright: false);
                await Cmd.CustomScaledWait(FastBounceTime, StandardBounceTime);
                VfxCmd.PlayOnCreatureCenter(target, BluntVfx);
                NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
            });
    }

    /// <summary>
    /// The point (global) where the attacker's center stops when it slams into the target: just short of the target's
    /// center, on the attacker's side.
    /// </summary>
    private static Vector2? ContactPoint(Creature attacker, Creature target)
    {
        if (VisualsOf(attacker) is not { } attackerVisuals || VisualsOf(target) is not { } targetVisuals) return null;
        var from = attackerVisuals.VfxSpawnPosition.GlobalPosition;
        var to = targetVisuals.VfxSpawnPosition.GlobalPosition;
        var direction = (to - from).Normalized();
        return to - direction * PinballContactDistance;
    }

    /// <summary>
    /// Picks a point on a screen edge (global coordinates) for the creature's center to collide with:
    /// the top edge when launched, then any other edge than the current one.
    /// </summary>
    private static Vector2? NextEdgePoint(Creature creature, bool launch)
    {
        if (VisualsOf(creature) is not { } visuals) return null;
        var state = States.GetOrCreateValue(visuals);
        var screen = visuals.GetCanvasTransform().AffineInverse() * visuals.GetViewportRect();
        var inner = screen.Grow(-EdgeMargin);

        int edge;
        if (launch) edge = 0;
        else
        {
            do edge = GD.RandRange(0, 3);
            while (edge == state.Edge);
        }
        state.Edge = edge;

        // Launched towards the upper corners rather than straight up
        var t = launch
            ? (GD.Randf() < 0.5f ? (float) GD.RandRange(0.05, 0.3) : (float) GD.RandRange(0.7, 0.95))
            : (float) GD.RandRange(0.1, 0.9);
        return edge switch
        {
            0 => new Vector2(Mathf.Lerp(inner.Position.X, inner.End.X, t), inner.Position.Y),
            1 => new Vector2(inner.End.X, Mathf.Lerp(inner.Position.Y, inner.End.Y, t * 0.85f)),
            2 => new Vector2(Mathf.Lerp(inner.Position.X, inner.End.X, t), inner.End.Y),
            _ => new Vector2(inner.Position.X, Mathf.Lerp(inner.Position.Y, inner.End.Y, t * 0.85f)),
        };
    }

    /// <summary>
    /// Flies the creature so that its center reaches <paramref name="to"/> (global), or back to its place when null,
    /// spinning around its center all the way.
    /// </summary>
    private static void Fly(Creature creature, Vector2? to, float duration, bool landUpright)
    {
        if (VisualsOf(creature) is not { } visuals || visuals.GetParent() is not CanvasItem parent) return;
        var state = StateOf(visuals);

        // Center of the body relative to the visuals' origin (its feet), unrotated, in the parent's coordinates
        var center = visuals.VfxSpawnPosition.Position * visuals.Scale;
        var toParent = parent.GetGlobalTransform().AffineInverse();

        var startCenter = visuals.Position + center.Rotated(visuals.Rotation);
        var endCenter = to is { } global ? toParent * global : center;
        var startAngle = state.Angle;
        var spin = Mathf.Tau * SpinSpeed * duration * (GD.Randf() < 0.5f ? 1f : -1f);
        var endAngle = startAngle + spin;
        if (landUpright) endAngle = Mathf.Round(endAngle / Mathf.Tau) * Mathf.Tau;

        state.Tween = visuals.CreateTween();
        state.Tween.TweenMethod(Callable.From<float>(k =>
        {
            if (!GodotObject.IsInstanceValid(visuals)) return;
            var c = startCenter.Lerp(endCenter, k);
            var angle = Mathf.Lerp(startAngle, endAngle, k);
            state.Angle = angle;
            visuals.Rotation = angle;
            visuals.Position = c - center.Rotated(angle);
        }), 0f, 1f, duration).SetTrans(landUpright ? Tween.TransitionType.Quad : Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.In);
        if (landUpright)
        {
            state.Tween.TweenCallback(Callable.From(() =>
            {
                visuals.Position = Vector2.Zero;
                visuals.Rotation = 0f;
                state.Angle = 0f;
                state.Edge = -1;
            }));
        }
    }
}
