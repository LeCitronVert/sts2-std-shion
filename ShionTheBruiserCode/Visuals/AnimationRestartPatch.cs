using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// BaseLib plays our animations through the AnimationPlayer under Visuals, whose tracks only set the
/// AnimatedSprite2D's <c>animation</c> property (or, without one, with <c>AnimatedSprite2D.Play(name)</c>).
/// Both do nothing when that animation is already playing. On multi-hit attacks, the next hit comes before
/// the attack animation ends, so the character only animated once for several hits.
/// This runs before BaseLib's own SetAnimationTrigger prefix: if the same animation is still playing,
/// it is rewound to its first frame and sped up so each hit gets a full loop.
/// It must keep playing: stopping it would freeze the sprite, since setting <c>animation</c> doesn't resume it.
/// shion_visuals.gd resets the speed when going back to idle.
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
public static class AnimationRestartPatch
{
    private const float RepeatSpeedScale = 2f;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Prefix(NCreature __instance, string trigger)
    {
        if (__instance.HasSpineAnimation || __instance.Visuals == null) return;

        var sprite = __instance.Visuals.GetCurrentBody() as AnimatedSprite2D;
        if (sprite?.SpriteFrames == null) return;

        var animation = FindAnimation(sprite.SpriteFrames, trigger);
        if (animation == null) return;

        if (sprite.IsPlaying() && sprite.Animation == animation)
        {
            sprite.SetFrameAndProgress(0, 0f);
            sprite.SpeedScale = RepeatSpeedScale;
        }
        else
        {
            sprite.SpeedScale = 1f;
        }
    }

    // Same names BaseLib tries, in the same order
    private static StringName? FindAnimation(SpriteFrames frames, string trigger)
    {
        string[] names = trigger switch
        {
            "Hit" => [trigger, "Hurt", "hit", "hurt"],
            "Dead" => [trigger, "Die", "dead", "die"],
            _ => [trigger, trigger.ToLowerInvariant()],
        };
        foreach (var name in names)
        {
            if (frames.HasAnimation(name)) return name;
        }
        return null;
    }
}
