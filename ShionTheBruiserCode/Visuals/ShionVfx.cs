using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// One-off visual effects of Shion's cards and powers. Visual only, never touches the game state.
/// <list type="bullet">
/// <item>Come What May's running speedlines (scenes/fx/come_what_may_speedlines.tscn), full screen behind Shion during the card's cast animation.</item>
/// <item>Spirit Burst's lightning (scenes/fx/spirit_burst_lightning.tscn), behind Shion across the whole screen while it
/// autoplays cards. Owned by <see cref="CreatureFxController"/>, which listens to <see cref="SpiritBurstAutoPlayChanged"/>.</item>
/// <item>Spirit Burst's strike (scenes/fx/spirit_burst_strike.tscn), on each target hit by a card it autoplayed.</item>
/// </list>
/// </summary>
public static class ShionVfx
{
    private static readonly string SpeedlinesScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "come_what_may_speedlines.tscn");
    private static readonly string SpiritBurstStrikeScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "spirit_burst_strike.tscn");

    /// <summary>Raised when Spirit Burst starts (true) or stops (false) autoplaying a card for a creature.</summary>
    public static event Action<Creature, bool>? SpiritBurstAutoPlayChanged;

    public static void SetSpiritBurstAutoPlaying(Creature creature, bool autoPlaying) =>
        SpiritBurstAutoPlayChanged?.Invoke(creature, autoPlaying);

    /// <summary>Full screen running speedlines, behind the creature.</summary>
    public static void PlayComeWhatMaySpeedlines(Creature creature)
    {
        var visuals = creature.GetCreatureNode()?.Visuals;
        if (visuals == null) return;

        var speedlines = GD.Load<PackedScene>(SpeedlinesScenePath).Instantiate<Node2D>();
        visuals.AddChild(speedlines);
        // Behind the creature and its auras; the scene stretches itself over the whole screen
        visuals.MoveChild(speedlines, 0);
        speedlines.Call("play");
    }

    /// <summary>Spirit Burst's strike on a target, for <c>AttackCommand.WithHitVfxNode</c>.</summary>
    public static Node2D? CreateSpiritBurstStrike(Creature target)
    {
        var targetNode = target.GetCreatureNode();
        if (targetNode == null) return null;

        var at = targetNode.Visuals.VfxSpawnPosition.GlobalPosition;
        var strike = GD.Load<PackedScene>(SpiritBurstStrikeScenePath).Instantiate<Node2D>();
        // play() places it in global coordinates, so it needs to be in the tree
        strike.Ready += () => strike.Call("play", at);
        return strike;
    }
}
