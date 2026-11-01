using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Stellar Strike: when it kills an enemy, the enemy's death particles explode outwards at once, together with the
/// rainbow explosion, instead of the usual slow fade after the death animation.
/// <list type="bullet">
/// <item>While <see cref="ExplodingDeath.Begin"/> marks a target, its death (<c>NCreature.StartDeathAnim</c>) is flagged.</item>
/// <item>A flagged creature skips the wait for the end of its death animation.</item>
/// <item>Its <c>NMonsterDeathVfx</c> bursts all its particles outwards and dissolves the body quickly.</item>
/// </list>
/// Visual only: nothing here touches the game state.
/// </summary>
public static class ExplodingDeath
{
    /// <summary>Creatures whose death, if it happens now, should explode (set while Stellar Strike's attack resolves).</summary>
    private static readonly HashSet<Creature> Marked = [];

    /// <summary>Creature nodes currently dying with an explosion.</summary>
    private static readonly HashSet<NCreature> Exploding = [];

    public static void Begin(Creature target) => Marked.Add(target);

    public static void End(Creature target) => Marked.Remove(target);

    private const float DissolveTime = 0.45f;
    /// <summary>The particles' lifetime is 1.1s at the scene's 1.5 speed scale.</summary>
    private const float ParticleLifetime = 1.1f;

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class StartDeathAnimPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NCreature __instance)
        {
            if (__instance.Entity == null || !Marked.Contains(__instance.Entity)) return;
            if (!Exploding.Add(__instance)) return;
            __instance.TreeExiting += () => Exploding.Remove(__instance);
        }
    }

    /// <summary>AnimDie waits for the death animation's remaining time + 0.5s: makes it 0.</summary>
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.GetCurrentAnimationTimeRemaining))]
    public static class AnimationTimeRemainingPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreature __instance, ref float __result)
        {
            if (Exploding.Contains(__instance)) __result = -0.5f;
        }
    }

    [HarmonyPatch(typeof(NMonsterDeathVfx), "PlayVfxInternal")]
    public static class DeathVfxPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NMonsterDeathVfx __instance, List<NCreature>? ____creatureNodes, ref Task __result)
        {
            var node = ____creatureNodes?.FirstOrDefault(Exploding.Contains);
            if (node == null) return true;
            Exploding.Remove(node);
            __result = Explode(__instance);
            return false;
        }
    }

    /// <summary>The base game's PlayVfxInternal, with the particles bursting all at once and a quick dissolve.</summary>
    private static async Task Explode(NMonsterDeathVfx vfx)
    {
        SfxCmd.Play("event:/sfx/enemy/enemy_fade");
        var viewport = vfx.GetNode<SubViewport>("Viewport");
        var visual = vfx.GetNode<Sprite2D>("Visual");
        var particles = vfx.GetNode<CpuParticles2D>("%Particles");

        particles.EmissionSphereRadius = viewport.Size.Y / 4f;
        particles.Explosiveness = 1f;
        particles.Direction = Vector2.Up;
        particles.Spread = 180f;
        particles.Gravity = new Vector2(0f, 400f);
        particles.InitialVelocityMin = 700f;
        particles.InitialVelocityMax = 1600f;
        particles.DampingMin = 600f;
        particles.DampingMax = 1100f;
        particles.Lifetime = ParticleLifetime * particles.SpeedScale;
        particles.LifetimeRandomness = 0.4f;
        particles.Emitting = true;

        using var tween = vfx.CreateTween();
        tween.TweenProperty(visual.Material, "shader_parameter/threshold", 0f, DissolveTime)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        // The particles are under the vfx: keep it until they're gone
        tween.TweenInterval(ParticleLifetime - DissolveTime);
        await vfx.ToSignal(tween, Tween.SignalName.Finished);
        if (GodotObject.IsInstanceValid(vfx)) vfx.QueueFree();
    }
}
