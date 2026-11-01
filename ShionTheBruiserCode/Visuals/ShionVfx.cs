using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// One-off visual effects of Shion's cards and powers. Visual only, never touches the game state
/// (no RNG, no commands): every client plays them on its own when the card resolves.
/// <list type="bullet">
/// <item>Come What May's running speedlines (scenes/fx/come_what_may_speedlines.tscn), full screen behind Shion during the card's cast animation.</item>
/// <item>Spirit Burst's lightning (scenes/fx/spirit_burst_lightning.tscn), behind Shion across the whole screen while it
/// autoplays cards, and once for Borrow Technique. Owned by <see cref="CreatureFxController"/>.</item>
/// <item>Spirit Burst's strike (scenes/fx/spirit_burst_strike.tscn), on each target hit by a card it autoplayed.</item>
/// <item>Team Blast's confetti, Energy Bomb's impact frame, Dragon Roar's shockwave, Dragon Pulse's wind gust,
/// the fiery auras of Energy Bind / Energy Burst / Berserk Flow Charge, Improvise's light bulb, the anger mark
/// (Competitive Spirit, Pout), Crunch's bite and Stellar Strike's rainbow explosion (with <see cref="ExplodingDeath"/>).</item>
/// <item>Events the <see cref="CreatureFxController"/> listens to: Nap / Eepy (Zs and slow animations),
/// Dragonsong's red flash.</item>
/// </list>
/// </summary>
public static class ShionVfx
{
    private static string FxScene(string name) => Path.Join(MainFile.ResPath, "scenes", "fx", name + ".tscn");

    private static readonly string SpeedlinesScenePath = FxScene("come_what_may_speedlines");
    private static readonly string SpiritBurstStrikeScenePath = FxScene("spirit_burst_strike");

    /// <summary>The base game's smoky vignette (Nightmare's), reused for Berserk Form.</summary>
    private const string SmokyVignetteScenePath = "res://scenes/vfx/whole_screen/vfx_smoky_vignette.tscn";

    /// <summary>Fur Coat's bite.</summary>
    private const string BiteVfx = "vfx/vfx_bite";

    /// <summary>Aura spritesheets (images/charui/shion/aura) used by <see cref="PlayFlameBurst"/>.</summary>
    public const string PurpleFlames = "fell_dragon";
    public const string OrangeFlames = "crit_above_50_or_vigor_above_20";

    /// <summary>Raised when Spirit Burst starts (true) or stops (false) autoplaying a card for a creature.</summary>
    public static event Action<Creature, bool>? SpiritBurstAutoPlayChanged;

    /// <summary>Raised to play Spirit Burst's lightning once (Borrow Technique).</summary>
    public static event Action<Creature>? SpiritBurstLightningOnce;

    /// <summary>Raised when Nap is played: Zs fly out and the creature's animations slow down until they're gone.</summary>
    public static event Action<Creature>? NapPlayed;

    /// <summary>Raised when Eepy is played: Zs and slow animations until the creature's next turn.</summary>
    public static event Action<Creature>? EepyPlayed;

    /// <summary>Raised when Dragonsong reduces damage taken by a creature (its notes flash red).</summary>
    public static event Action<Creature>? DragonsongReduced;

    public static void SetSpiritBurstAutoPlaying(Creature creature, bool autoPlaying) =>
        SpiritBurstAutoPlayChanged?.Invoke(creature, autoPlaying);

    public static void PlaySpiritBurstLightningOnce(Creature creature) => SpiritBurstLightningOnce?.Invoke(creature);

    public static void PlayNap(Creature creature) => NapPlayed?.Invoke(creature);

    public static void PlayEepy(Creature creature) => EepyPlayed?.Invoke(creature);

    public static void FlashDragonsong(Creature creature) => DragonsongReduced?.Invoke(creature);

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

    /// <summary>
    /// Stellar Strike's rainbow explosion at the center of a target, for <c>AttackCommand.WithHitVfxNode</c>.
    /// If the hit kills, <see cref="ExplodingDeath"/> makes its death particles explode at the same time.
    /// </summary>
    public static Node2D? CreateStellarExplosion(Creature target)
    {
        var targetNode = target.GetCreatureNode();
        if (targetNode == null || TestMode.IsOn) return null;

        var at = targetNode.Visuals.VfxSpawnPosition.GlobalPosition;
        var size = targetNode.Visuals.Bounds.GetGlobalRect().Size;
        var radius = Math.Max(size.X, size.Y) * 0.6f;
        var explosion = GD.Load<PackedScene>(FxScene("rainbow_explosion")).Instantiate<Node2D>();
        // play() places it in global coordinates, so it needs to be in the tree
        explosion.Ready += () => explosion.Call("play", at, radius);
        NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
        return explosion;
    }

    // ---------------------------------------------------------------- Screen-wide

    /// <summary>Screen space layer over the whole run (where Nightmare puts its vignette).</summary>
    private static Control? ScreenLayer =>
        TestMode.IsOn ? null : NGame.Instance?.CurrentRunNode?.GlobalUi;

    /// <summary>Team Blast (Dragon Cheer): confetti falling from the top of the screen.</summary>
    public static void PlayConfetti()
    {
        var layer = ScreenLayer;
        if (layer == null) return;
        var confetti = GD.Load<PackedScene>(FxScene("confetti")).Instantiate<Node2D>();
        layer.AddChild(confetti);
        confetti.Call("play");
    }

    /// <summary>
    /// Energy Bomb's impact frame: for a split second the background turns white and every creature is drawn
    /// as a black outline, then a white flash fades out.
    /// </summary>
    public static void PlayImpactFrame()
    {
        var room = NCombatRoom.Instance;
        var layer = ScreenLayer;
        if (room == null || layer == null) return;

        const string key = "impact_frame";
        const float hold = 0.12f;

        // White backdrop over the background, behind the creatures
        var backdrop = new ColorRect
        {
            Color = Colors.White,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(-5000f, -5000f),
            Size = new Vector2(10000f, 10000f),
        };
        room.BackCombatVfxContainer.AddChild(backdrop);

        var creatures = room.CreatureNodes.Where(GodotObject.IsInstanceValid).ToList();
        foreach (var node in creatures)
        {
            CreatureLook.SetMaterial(node, key, CreatureLook.ImpactFramePriority, CreatureLook.Silhouette);
        }

        // Real time, so hit stop (which slows the engine down) doesn't stretch it
        var timer = layer.GetTree().CreateTimer(hold, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
        timer.Timeout += () =>
        {
            if (GodotObject.IsInstanceValid(backdrop)) backdrop.QueueFree();
            foreach (var node in creatures)
            {
                CreatureLook.SetMaterial(node, key, 0, null);
            }
            if (!GodotObject.IsInstanceValid(layer)) return;
            var flash = GD.Load<PackedScene>(FxScene("impact_frame")).Instantiate<Node2D>();
            layer.AddChild(flash);
            flash.Call("play");
        };
    }

    /// <summary>
    /// Berserk Form: the base game's smoky vignette (Nightmare's), tinted crimson, but without its own script, which
    /// fades it out after a second. The caller adds it to <see cref="ScreenLayer"/> and drives its alpha.
    /// </summary>
    public static Control? CreateBerserkVignette()
    {
        if (ScreenLayer == null) return null;
        var source = GD.Load<PackedScene>(SmokyVignetteScenePath).Instantiate<Control>();

        var vignette = new Control
        {
            Name = "BerserkFormVignette",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = source.Material,
            Modulate = new Color(0.86f, 0.08f, 0.24f, 0f), // Crimson
        };
        vignette.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        foreach (var child in source.GetChildren().ToList())
        {
            source.RemoveChild(child);
            vignette.AddChild(child);
            if (child is Control control) control.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        source.Free();

        if (vignette.GetNodeOrNull<CanvasItem>("Highlights") is { } highlights)
        {
            highlights.Modulate = new Color(3f, 0.15f, 0.35f, 0.33f);
        }
        return vignette;
    }

    public static void AddToScreen(Node node) => ScreenLayer?.AddChild(node);

    // ---------------------------------------------------------------- On a creature

    private static (NCreatureVisuals Visuals, Rect2 Bounds)? Locate(Creature creature)
    {
        if (TestMode.IsOn) return null;
        var visuals = creature.GetCreatureNode()?.Visuals;
        if (visuals == null || !GodotObject.IsInstanceValid(visuals)) return null;
        return (visuals, new Rect2(visuals.Bounds.Position, visuals.Bounds.Size));
    }

    /// <summary>Top of the head, slightly towards the front (creatures face right on the player side).</summary>
    public static Vector2 HeadTop(Rect2 bounds, bool facesRight) =>
        new(bounds.GetCenter().X + (facesRight ? 1f : -1f) * bounds.Size.X * 0.08f, bounds.Position.Y + bounds.Size.Y * 0.08f);

    private static Node2D Spawn(NCreatureVisuals visuals, string scene)
    {
        var node = GD.Load<PackedScene>(FxScene(scene)).Instantiate<Node2D>();
        visuals.AddChild(node);
        return node;
    }

    /// <summary>Crunch: Fur Coat's bite on the target.</summary>
    public static void PlayBite(Creature target) => VfxCmd.PlayOnCreatureCenter(target, BiteVfx);

    /// <summary>Competitive Spirit's proc, Pout: 💢 on the head.</summary>
    public static void PlayAngerMark(Creature creature)
    {
        if (Locate(creature) is not { } located) return;
        var (visuals, bounds) = located;
        var at = HeadTop(bounds, creature.IsPlayer) + new Vector2((creature.IsPlayer ? 1f : -1f) * bounds.Size.X * 0.22f, 0f);
        Spawn(visuals, "anger_mark").Call("play", at);
    }

    /// <summary>Improvise: a light bulb above the head, which explodes.</summary>
    public static void PlayLightBulb(Creature creature)
    {
        if (Locate(creature) is not { } located) return;
        var (visuals, bounds) = located;
        Spawn(visuals, "light_bulb").Call("play", new Vector2(bounds.GetCenter().X, bounds.Position.Y - 55f));
    }

    /// <summary>Energy Bind / Energy Burst / Berserk Flow Charge: fiery aura for a few seconds.</summary>
    public static void PlayFlameBurst(Creature creature, string sheet)
    {
        if (Locate(creature) is not { } located) return;
        var (visuals, bounds) = located;
        var flames = Spawn(visuals, "flame_burst");
        // Behind the creature
        visuals.MoveChild(flames, 0);
        flames.Call("play", sheet, new Vector2(bounds.GetCenter().X, bounds.End.Y + 14f), bounds.Size.Y);
    }

    /// <summary>Dragon Roar: purple shockwave from the mouth towards the enemies, and a screen shake.</summary>
    public static void PlayDragonRoar(Creature creature)
    {
        if (Locate(creature) is not { } located) return;
        var (visuals, bounds) = located;
        var mouth = new Vector2(bounds.End.X - bounds.Size.X * 0.25f, bounds.Position.Y + bounds.Size.Y * 0.2f);
        var roar = Spawn(visuals, "dragon_roar");
        roar.Position = mouth;
        roar.Call("play");
        NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
    }

    /// <summary>Dragon Pulse: translucent purple wind gusting out of Shion.</summary>
    public static void PlayDragonPulse(Creature creature)
    {
        if (Locate(creature) is not { } located) return;
        var (visuals, bounds) = located;
        var gust = Spawn(visuals, "wind_gust");
        gust.Position = bounds.GetCenter();
        gust.Call("play");
    }
}
