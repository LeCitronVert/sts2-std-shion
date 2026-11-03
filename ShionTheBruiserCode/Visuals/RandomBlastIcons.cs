using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Random Blast's proc: a random buff/debuff icon (any power of the game, mods included) is thrown out of the
/// creature, spinning, towards the enemies: somewhere between straight ahead and 35° up, until it leaves the screen.
/// Purely visual: uses its own RNG (never the run's), so it doesn't affect multiplayer sync.
/// </summary>
public static class RandomBlastIcons
{
    /// <summary>Highest angle above straight ahead.</summary>
    private const float MaxAngleDegrees = 35f;

    /// <summary>On-screen size of the icon (its longest side).</summary>
    private const float IconSize = 80f;

    /// <summary>Pixels per second.</summary>
    private const float Speed = 2000f;

    /// <summary>Turns per second.</summary>
    private const float TurnsPerSecond = 2.5f;

    /// <summary>Extra distance past the screen's edge, so the whole icon is gone.</summary>
    private const float OffscreenMargin = 150f;

    private static readonly Random Rng = new();

    private static List<PowerModel>? _pool;

    private static List<PowerModel> Pool => _pool ??= ModelDb.AllPowers
        .Where(power => power.Type is PowerType.Buff or PowerType.Debuff)
        .ToList();

    public static void Throw(Creature from)
    {
        if (TestMode.IsOn) return;
        var room = NCombatRoom.Instance;
        var node = from.GetCreatureNode();
        if (room == null || node == null || !GodotObject.IsInstanceValid(node)) return;

        var texture = RandomIcon();
        if (texture == null) return;

        var start = node.VfxSpawnPosition;
        var forward = from.IsPlayer ? 1f : -1f;
        var angle = Mathf.DegToRad((float) Rng.NextDouble() * MaxAngleDegrees);
        // Up on screen is -Y
        var direction = new Vector2(Mathf.Cos(angle) * forward, -Mathf.Sin(angle));

        // Far enough to leave the screen on the enemies' side
        var viewport = node.GetViewportRect();
        var screenEdge = node.GetCanvasTransform().AffineInverse() * (forward > 0 ? viewport.End : viewport.Position);
        var horizontal = Mathf.Max((screenEdge.X - start.X) * forward, 0f) + OffscreenMargin;
        var distance = horizontal / Mathf.Cos(angle);
        var duration = distance / Speed;

        var sprite = new Sprite2D
        {
            Texture = texture,
            Scale = Vector2.One * (IconSize / Mathf.Max(texture.GetWidth(), texture.GetHeight())),
        };
        room.CombatVfxContainer.AddChildSafely(sprite);
        sprite.GlobalPosition = start;

        var spin = Mathf.Tau * TurnsPerSecond * duration * (Rng.Next(2) == 0 ? 1f : -1f);
        var end = start + direction * distance;

        var tween = sprite.CreateTween().SetParallel();
        tween.TweenProperty(sprite, "global_position", end, duration);
        tween.TweenProperty(sprite, "rotation", spin, duration);
        tween.Chain().TweenCallback(Callable.From(sprite.QueueFree));
    }

    /// <summary>A random power's icon, skipping the ones that fail to load.</summary>
    private static Texture2D? RandomIcon()
    {
        var pool = Pool;
        for (var attempt = 0; attempt < 5 && pool.Count > 0; attempt++)
        {
            try
            {
                var icon = pool[Rng.Next(pool.Count)].Icon;
                if (icon != null) return icon;
            }
            catch (Exception)
            {
                // Missing icon (e.g. a mod's power): try another one
            }
        }
        return null;
    }
}
