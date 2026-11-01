using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Visual-only changes to a creature's body that several effects can ask for at the same time:
/// <list type="bullet">
/// <item>Animation speed (Eleventh Hour, Extreme Speed, Nap, Eepy). The slowest requested speed wins.
/// Spine bodies use their animation state's time scale; AnimatedSprite2D bodies (Shion) their speed scale, also
/// read by <see cref="AnimationRestartPatch"/> and shion_visuals.gd through the <see cref="SpeedMeta"/> meta.</item>
/// <item>Material overrides (Eleventh Hour's grayscale, Energy Bomb's impact frame). The highest priority wins, and the
/// body's own material comes back once none is left.</item>
/// </list>
/// Never touches the game state.
/// </summary>
public static class CreatureLook
{
    /// <summary>Meta on AnimatedSprite2D bodies holding the speed multiplier, read by shion_visuals.gd.</summary>
    public const string SpeedMeta = "fx_anim_speed";

    private static readonly string GrayscaleShaderPath = Path.Join(MainFile.ResPath, "scenes", "fx", "grayscale.gdshader");
    private static readonly string SilhouetteShaderPath = Path.Join(MainFile.ResPath, "scenes", "fx", "impact_silhouette.gdshader");

    public const int GrayscalePriority = 0;
    public const int ImpactFramePriority = 10;

    private class State
    {
        public readonly Dictionary<string, float> Speeds = new();
        public readonly Dictionary<string, (int Priority, Material Material)> Materials = new();
        public bool HasOriginalMaterial;
        public Material? OriginalMaterial;
        public float AppliedSpeed = 1f;
    }

    private static readonly ConditionalWeakTable<NCreature, State> States = new();

    private static Material? _grayscale;
    private static Material? _silhouette;

    public static Material Grayscale => _grayscale ??= new ShaderMaterial { Shader = GD.Load<Shader>(GrayscaleShaderPath) };
    public static Material Silhouette => _silhouette ??= new ShaderMaterial { Shader = GD.Load<Shader>(SilhouetteShaderPath) };

    /// <summary>Requests (or with null, removes) an animation speed multiplier for <paramref name="key"/>.</summary>
    public static void SetSpeed(NCreature node, string key, float? speed)
    {
        if (!GodotObject.IsInstanceValid(node) || node.Visuals == null) return;
        var state = States.GetOrCreateValue(node);
        if (speed is { } s) state.Speeds[key] = s;
        else if (!state.Speeds.Remove(key)) return;

        var target = state.Speeds.Count == 0 ? 1f : state.Speeds.Values.Min();
        if (Mathf.IsEqualApprox(target, state.AppliedSpeed)) return;
        var previous = state.AppliedSpeed;
        state.AppliedSpeed = target;

        var body = node.Visuals.GetCurrentBody();
        if (node.Visuals.SpineBody is { } spine)
        {
            spine.TryGetAnimationState()?.SetTimeScale(target);
        }
        else if (body is AnimatedSprite2D sprite)
        {
            sprite.SetMeta(SpeedMeta, target);
            // Keeps the multi-hit speed-up (AnimationRestartPatch) if it's running
            sprite.SpeedScale = sprite.SpeedScale / previous * target;
        }
    }

    /// <summary>Speed multiplier of an AnimatedSprite2D body (1 when nothing slows it).</summary>
    public static float GetSpriteSpeed(AnimatedSprite2D sprite) =>
        sprite.HasMeta(SpeedMeta) ? sprite.GetMeta(SpeedMeta).AsSingle() : 1f;

    /// <summary>Requests (or with null, removes) a material override for <paramref name="key"/>.</summary>
    public static void SetMaterial(NCreature node, string key, int priority, Material? material)
    {
        if (!GodotObject.IsInstanceValid(node) || node.Visuals == null) return;
        var state = States.GetOrCreateValue(node);
        if (material != null) state.Materials[key] = (priority, material);
        else if (!state.Materials.Remove(key)) return;

        var body = node.Visuals.GetCurrentBody();
        if (!GodotObject.IsInstanceValid(body)) return;
        var spine = node.Visuals.SpineBody;

        if (!state.HasOriginalMaterial)
        {
            state.OriginalMaterial = spine != null ? spine.GetNormalMaterial() : body.Material;
            state.HasOriginalMaterial = true;
        }

        var wanted = state.Materials.Count == 0
            ? state.OriginalMaterial
            : state.Materials.Values.MaxBy(m => m.Priority).Material;

        if (spine != null) body.Call("set_normal_material", wanted == null ? new Variant() : wanted);
        else body.Material = wanted;

        if (state.Materials.Count == 0)
        {
            state.HasOriginalMaterial = false;
            state.OriginalMaterial = null;
        }
    }
}
