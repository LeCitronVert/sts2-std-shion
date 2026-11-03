using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Played before every Team Blast resolves: every living player of the team is surrounded by a gentle light (colored
/// by character), then after <see cref="LightToEntranceDelay"/> the base game's Dramatic Entrance effect plays,
/// then the Team Blast itself. Run from <see cref="Singletons.TeamBlastSingleton.BeforeCardPlayed"/>, on every client.
/// The lights are visual only; the waits go through <see cref="Cmd.Wait(float, bool)"/> like every gameplay wait.
/// </summary>
public static class TeamBlastIntro
{
    /// <summary>The base game's Dramatic Entrance full screen effect.</summary>
    private const string DramaticEntranceVfx = "vfx/vfx_dramatic_entrance_fullscreen";

    public const float LightToEntranceDelay = 0.5f;

    /// <summary>Lets the Dramatic Entrance effect get going before the Team Blast proper.</summary>
    public const float EntranceToBlastDelay = 0.25f;

    // Light: bursts out of the character, expanding and fading out by the time Dramatic Entrance plays
    /// <summary>Time to reach full brightness, right at the start.</summary>
    private const float FlashIn = 0.06f;
    /// <summary>Starting / final size, relative to the character's size.</summary>
    private const float StartSize = 0.25f;
    private const float EndSize = 2.6f;

    private static readonly Color Blue = new(0.45f, 0.7f, 1f);
    private static readonly Color Red = new(1f, 0.4f, 0.35f);
    private static readonly Color Yellow = new(1f, 0.85f, 0.4f);
    private static readonly Color Neutral = new(1f, 1f, 0.95f);

    private static Texture2D? _glowTexture;

    public static Color LightColor(CharacterModel? character) => character switch
    {
        Silent or Defect or SlalTheEndless => Blue,
        Ironclad or FryTheCommander or Character.ShionTheBruiser => Red,
        Regent or SeelsTheReckless or Necrobinder => Yellow,
        _ => Neutral,
    };

    public static async Task Play(Player owner, IEnumerable<Creature> players)
    {
        foreach (var creature in players)
        {
            PlayLight(creature);
        }

        await Cmd.Wait(LightToEntranceDelay);
        VfxCmd.PlayFullScreenInCombat(DramaticEntranceVfx, owner.Creature);
        await Cmd.Wait(EntranceToBlastDelay);
    }

    /// <summary>
    /// A soft radial glow behind the creature, with a fainter one over it, starting small at its center and expanding
    /// while fading out over <see cref="LightToEntranceDelay"/>.
    /// </summary>
    private static void PlayLight(Creature creature)
    {
        if (TestMode.IsOn) return;
        var visuals = creature.GetCreatureNode()?.Visuals;
        if (visuals == null || !GodotObject.IsInstanceValid(visuals)) return;

        var bounds = new Rect2(visuals.Bounds.Position, visuals.Bounds.Size);
        var color = LightColor(creature.Player?.Character);
        var size = Math.Max(bounds.Size.X, bounds.Size.Y);

        var back = CreateGlow(color, size, bounds.GetCenter(), 0.85f);
        visuals.AddChild(back);
        visuals.MoveChild(back, 0);

        var front = CreateGlow(color, size * 0.9f, bounds.GetCenter(), 0.22f);
        visuals.AddChild(front);

        Animate(back);
        Animate(front);
    }

    private static Sprite2D CreateGlow(Color color, float size, Vector2 at, float alpha)
    {
        var texture = GlowTexture;
        var sprite = new Sprite2D
        {
            Texture = texture,
            Position = at,
            Scale = Vector2.One * (size / texture.GetWidth()),
            Modulate = new Color(color, 0f),
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add },
        };
        sprite.SetMeta("peak_alpha", alpha);
        return sprite;
    }

    private static void Animate(Sprite2D sprite)
    {
        var peak = sprite.GetMeta("peak_alpha").AsSingle();
        var fullScale = sprite.Scale;
        sprite.Scale = fullScale * StartSize;

        var tween = sprite.CreateTween().SetParallel();
        tween.TweenProperty(sprite, "scale", fullScale * EndSize, LightToEntranceDelay)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(sprite, "modulate:a", peak, FlashIn);
        tween.TweenProperty(sprite, "modulate:a", 0f, LightToEntranceDelay - FlashIn).SetDelay(FlashIn)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(sprite.QueueFree));
    }

    /// <summary>White radial gradient, opaque at the center and fully transparent at the edge.</summary>
    private static Texture2D GlowTexture => _glowTexture ??= new GradientTexture2D
    {
        Width = 256,
        Height = 256,
        Fill = GradientTexture2D.FillEnum.Radial,
        FillFrom = new Vector2(0.5f, 0.5f),
        FillTo = new Vector2(0.5f, 0f),
        Gradient = new Gradient
        {
            Offsets = [0f, 0.45f, 1f],
            Colors = [new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.45f), new Color(1f, 1f, 1f, 0f)],
        },
    };
}
