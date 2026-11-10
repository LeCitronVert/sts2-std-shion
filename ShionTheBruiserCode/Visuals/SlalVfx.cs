using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// One-off visual effects of Slal's cards, drawn in code (scenes/fx/*.gd). Visual only, never touches the game state:
/// every client plays them on its own when the card resolves.
/// <list type="bullet">
/// <item>Elaborate Flourish: Slal whirls on herself with colorful streaks (scenes/fx/whirl.tscn).</item>
/// <item>Anvil Gag / Funny Gag: an anvil or a flower pot falls on the target's head (scenes/fx/falling_gag.tscn).</item>
/// </list>
/// </summary>
public static class SlalVfx
{
    private static string FxScene(string name) => Path.Join(MainFile.ResPath, "scenes", "fx", name + ".tscn");

    /// <summary>Time the falling gags take to reach the head (falling_gag.gd's fall_time).</summary>
    public const float GagFallTime = 0.42f;

    /// <summary>Time Elaborate Flourish's whirl lasts (whirl.gd's duration).</summary>
    public const float WhirlTime = 0.75f;

    /// <summary>How far above the top of the screen the gags start.</summary>
    private const float AboveScreen = 150f;

    public enum Gag
    {
        Anvil,
        FlowerPot,
    }

    private static (NCreatureVisuals Visuals, Rect2 Bounds)? Locate(Creature creature)
    {
        if (TestMode.IsOn) return null;
        var visuals = creature.GetCreatureNode()?.Visuals;
        if (visuals == null || !GodotObject.IsInstanceValid(visuals)) return null;
        return (visuals, new Rect2(visuals.Bounds.Position, visuals.Bounds.Size));
    }

    private static Node2D Spawn(NCreatureVisuals visuals, string scene)
    {
        var node = GD.Load<PackedScene>(FxScene(scene)).Instantiate<Node2D>();
        visuals.AddChild(node);
        return node;
    }

    /// <summary>Elaborate Flourish: Slal spins like a top. Returns false if nothing was shown.</summary>
    public static bool PlayWhirl(Creature creature)
    {
        if (Locate(creature) is not { } located) return false;
        var (visuals, bounds) = located;
        Spawn(visuals, "whirl").Call("play", bounds.GetCenter(), bounds.Size);
        return true;
    }

    /// <summary>
    /// Anvil Gag / Funny Gag: the object falls from above the screen onto the target's head.
    /// Returns false if nothing was shown (then there's nothing to wait for).
    /// </summary>
    public static bool PlayFallingGag(Creature target, Gag gag)
    {
        if (Locate(target) is not { } located) return false;
        var (visuals, bounds) = located;
        var head = ShionVfx.HeadTop(bounds, target.IsPlayer);

        // Starts just above the top of the screen, whatever the creature's size or scale
        var screenTop = visuals.GetCanvasTransform().AffineInverse() * visuals.GetViewportRect().Position;
        var start = visuals.ToLocal(new Vector2(visuals.ToGlobal(head).X, screenTop.Y - AboveScreen));
        var fallHeight = Mathf.Max(head.Y - start.Y, 300f);

        var kind = gag == Gag.Anvil ? "anvil" : "flower_pot";
        Spawn(visuals, "falling_gag").Call("play", kind, head, fallHeight);
        return true;
    }
}
