using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Moves a player's whole creature node (body, health bar, hitbox) to another place while a power lasts, then back home.
/// Driven by <see cref="CreatureFxController"/> on every combat state change.
/// <list type="bullet">
/// <item>Full Nelson: right behind the enemy that has her Full Nelson, facing the player team (flipped, effects included).</item>
/// <item>Frontliner: in front of her team, facing the enemies.</item>
/// </list>
/// Visual only: nothing here touches the game state.
/// </summary>
public class CreaturePositioning(NCreature node)
{
    private const float MoveTime = 0.35f;
    /// <summary>How much of her width overlaps the held enemy's back.</summary>
    private const float FullNelsonOverlap = 0.3f;
    /// <summary>Gap between her and the front ally (Frontliner), and how far forward she steps at least.</summary>
    private const float FrontGap = 50f;
    private const float MinFrontStep = 150f;
    /// <summary>Gap kept with the enemies and the screen edges.</summary>
    private const float Margin = 40f;

    private Vector2? _home;
    private Vector2? _target;
    private Tween? _tween;
    private bool _flipped;

    public void Refresh()
    {
        if (!GodotObject.IsInstanceValid(node) || node.Visuals == null) return;
        var creature = node.Entity;

        Vector2? target = null;
        var flip = false;
        if (creature.IsAlive && FullNelsonTarget(creature) is { } held && HeldPosition(held) is { } behind)
        {
            target = behind;
            flip = true;
        }
        else if (creature.IsAlive && creature.HasPower<Frontliner>())
        {
            target = FrontPosition(creature);
        }

        SetFlipped(flip);
        MoveTo(target);
    }

    /// <summary>The enemy holding this creature's Full Nelson (it applies Full Nelson to both).</summary>
    private static Creature? FullNelsonTarget(Creature creature)
    {
        if (!creature.HasPower<FullNelson>() || creature.CombatState == null) return null;
        return creature.CombatState.GetOpponentsOf(creature)
            .FirstOrDefault(e => e.IsAlive && e.GetPower<FullNelson>()?.Applier == creature);
    }

    private Rect2 OwnRect => node.Visuals.Bounds.GetGlobalRect().Abs();

    /// <summary>Visible area, in the same (canvas) coordinates as the global rects.</summary>
    private Rect2 Screen => node.GetCanvasTransform().AffineInverse() * node.GetViewportRect();

    /// <summary>Global position for the node so its bounds' bottom center lands at <paramref name="feet"/>.</summary>
    private Vector2 NodePositionFor(Vector2 feet)
    {
        var rect = OwnRect;
        var current = new Vector2(rect.GetCenter().X, rect.End.Y);
        return node.GlobalPosition + (feet - current);
    }

    /// <summary>Right behind the held enemy (on its far side from the players), feet level with its own.</summary>
    private Vector2? HeldPosition(Creature enemy)
    {
        var enemyNode = enemy.GetCreatureNode();
        if (enemyNode?.Visuals == null || !GodotObject.IsInstanceValid(enemyNode)) return null;
        var enemyRect = enemyNode.Visuals.Bounds.GetGlobalRect().Abs();
        var width = OwnRect.Size.X;
        var x = enemyRect.End.X + width * (0.5f - FullNelsonOverlap);
        x = Mathf.Min(x, Screen.End.X - width * 0.5f - Margin);
        return NodePositionFor(new Vector2(x, enemyRect.End.Y));
    }

    /// <summary>Ahead of the front ally, without walking into the enemies.</summary>
    private Vector2 FrontPosition(Creature creature)
    {
        var width = OwnRect.Size.X;
        var homeFeet = HomeFeet();
        var room = NCombatRoom.Instance;
        var allies = room?.CreatureNodes
            .Where(n => GodotObject.IsInstanceValid(n) && n != node && n.Entity.IsAlive && n.Entity.Side == creature.Side)
            .ToList() ?? [];
        var enemies = room?.CreatureNodes
            .Where(n => GodotObject.IsInstanceValid(n) && n.Entity.IsAlive && n.Entity.Side != creature.Side)
            .ToList() ?? [];

        var front = allies.Count > 0 ? allies.Max(n => n.Visuals.Bounds.GetGlobalRect().Abs().End.X) : homeFeet.X;
        var x = Mathf.Max(front + FrontGap + width * 0.5f, homeFeet.X + MinFrontStep);
        if (enemies.Count > 0)
        {
            var enemyFront = enemies.Min(n => n.Visuals.Bounds.GetGlobalRect().Abs().Position.X);
            x = Mathf.Min(x, enemyFront - Margin - width * 0.5f);
        }
        return NodePositionFor(new Vector2(x, homeFeet.Y));
    }

    /// <summary>Bottom center of her bounds when she's home.</summary>
    private Vector2 HomeFeet()
    {
        var rect = OwnRect;
        var feet = new Vector2(rect.GetCenter().X, rect.End.Y);
        if (_home is not { } home || node.GetParent() is not CanvasItem parent) return feet;
        return feet + (parent.GetGlobalTransform() * home - node.GlobalPosition);
    }

    private void MoveTo(Vector2? globalTarget)
    {
        if (globalTarget is { } target)
        {
            _home ??= node.Position;
            if (_target is { } current && current.DistanceTo(target) < 2f) return;
            _target = target;
            StartTween(n => n.TweenProperty(node, "global_position", target, MoveTime));
        }
        else
        {
            if (_home is not { } home) return;
            _home = null;
            _target = null;
            StartTween(n => n.TweenProperty(node, "position", home, MoveTime));
        }
    }

    private void StartTween(Action<Tween> setup)
    {
        if (_tween != null && _tween.IsValid()) _tween.Kill();
        _tween = node.CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        setup(_tween);
    }

    /// <summary>
    /// Faces left (towards the players) when flipped. The whole visuals node is mirrored (around its origin, at her feet),
    /// so every effect attached to her (Defensive's shell, auras, sweat drops, 💢...) is mirrored with her.
    /// <see cref="FlipPatches"/> keeps the base game's scale code from undoing it or reading it as a negative size.
    /// </summary>
    private void SetFlipped(bool flipped)
    {
        if (flipped == _flipped) return;
        _flipped = flipped;
        var visuals = node.Visuals;
        if (flipped)
        {
            if (Flipped.Add(visuals)) visuals.TreeExiting += () => Flipped.Remove(visuals);
        }
        else Flipped.Remove(visuals);
        ApplyFlip(visuals);
    }

    private static readonly HashSet<NCreatureVisuals> Flipped = [];

    public static bool IsFlipped(NCreatureVisuals visuals) => Flipped.Contains(visuals);

    /// <summary>Makes the visuals' horizontal scale negative when flipped, positive otherwise.</summary>
    public static void ApplyFlip(NCreatureVisuals visuals)
    {
        if (!GodotObject.IsInstanceValid(visuals)) return;
        var scale = visuals.Scale;
        var x = Mathf.Abs(scale.X) * (Flipped.Contains(visuals) ? -1f : 1f);
        if (!Mathf.IsEqualApprox(x, scale.X)) visuals.Scale = new Vector2(x, scale.Y);
    }

    /// <summary>Removes the flip for the duration of base game code that reads the visuals' scale as a size.</summary>
    public static void Unflip(NCreatureVisuals? visuals)
    {
        if (visuals == null || !Flipped.Contains(visuals) || !GodotObject.IsInstanceValid(visuals)) return;
        visuals.Scale = new Vector2(Mathf.Abs(visuals.Scale.X), visuals.Scale.Y);
    }
}

/// <summary>Keeps <see cref="CreaturePositioning"/>'s flip through the base game's scale changes. Visual only.</summary>
public static class FlipPatches
{
    /// <summary>Scale tweens (size changing powers) set a positive scale every frame.</summary>
    [HarmonyPatch(typeof(NCreature), "DoScaleTween")]
    public static class DoScaleTweenPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NCreature __instance, ref Vector2 scale)
        {
            if (__instance.Visuals != null && CreaturePositioning.IsFlipped(__instance.Visuals))
            {
                scale = new Vector2(-Mathf.Abs(scale.X), scale.Y);
            }
        }
    }

    [HarmonyPatch(typeof(NCreatureVisuals), nameof(NCreatureVisuals.SetScaleAndHue))]
    public static class SetScaleAndHuePatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreatureVisuals __instance) => CreaturePositioning.ApplyFlip(__instance);
    }

    /// <summary>Hitbox / intent placement and orb placement read the scale as a size: unflipped while they run.</summary>
    [HarmonyPatch]
    public static class SizeReadersPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<System.Reflection.MethodBase> TargetMethods() =>
        [
            AccessTools.Method(typeof(NCreature), "UpdateBounds", [typeof(Node)]),
            AccessTools.Method(typeof(NCreature), "SetOrbManagerPosition"),
        ];

        [HarmonyPrefix]
        public static void Prefix(NCreature __instance) => CreaturePositioning.Unflip(__instance.Visuals);

        [HarmonyPostfix]
        public static void Postfix(NCreature __instance)
        {
            if (__instance.Visuals != null) CreaturePositioning.ApplyFlip(__instance.Visuals);
        }
    }
}
