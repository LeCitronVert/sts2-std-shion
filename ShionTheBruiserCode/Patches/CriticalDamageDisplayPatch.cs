using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Patches NDamageNumVfx to customize critical hit damage displays.
/// Critical hits show a "!" suffix and are colored:
/// - Gold if unblocked crit
/// - Orange if fully blocked crit
/// </summary>
[HarmonyPatch(typeof(NDamageNumVfx))]
public class CriticalDamageDisplayPatch
{
    // Storage for critical hit information
    private static readonly System.Collections.Generic.Dictionary<NDamageNumVfx, (bool isCrit, bool isBlocked)> _criticalHitInfo = 
        new System.Collections.Generic.Dictionary<NDamageNumVfx, (bool, bool)>();

    /// <summary>
    /// Postfix patch on NDamageNumVfx.Create(Creature, DamageResult).
    /// Modifies the damage display text to add "!" for critical hits
    /// and stores crit information for color customization.
    /// </summary>
    [HarmonyPatch(nameof(NDamageNumVfx.Create), new[] { typeof(Creature), typeof(DamageResult) })]
    [HarmonyPostfix]
    public static void CreatePostfix(Creature target, DamageResult result, ref NDamageNumVfx __result)
    {
        // Early exit if result is null (can happen in test mode)
        if (__result is null)
            return;

        // Early exit if singleton not initialized
        if (ShionCritSingleton.Instance is null)
            return;

        // Early exit if not a critical hit
        if (!ShionCritSingleton.Instance.CurrentHitIsCrit)
            return;

        MainFile.Logger.Info($"[CriticalDamageDisplay] CRITICAL HIT DETECTED! Customizing damage display...");
        ApplyCriticalHitStyle(__result, result);
    }

    /// <summary>
    /// Replaces NDamageNumVfx.AnimVfx for critical hits with a gold/orange version of the same animation.
    /// Vanilla awaits its own tween and frees the node once it ends, and a killed tween counts as ended,
    /// so killing it to recolor (as this patch used to) freed the number one frame after it spawned.
    /// </summary>
    [HarmonyPatch("AnimVfx")]
    [HarmonyPrefix]
    public static bool AnimVfxPrefix(NDamageNumVfx __instance, ref Task __result)
    {
        if (!_criticalHitInfo.Remove(__instance, out var critInfo) || !critInfo.isCrit)
            return true;

        __result = CritAnimVfx(__instance, critInfo.isBlocked);
        return false;
    }

    private static readonly AccessTools.FieldRef<NDamageNumVfx, Tween?> TweenRef =
        AccessTools.FieldRefAccess<NDamageNumVfx, Tween?>("_tween");

    private static async Task CritAnimVfx(NDamageNumVfx vfx, bool isBlocked)
    {
        Color critColor = isBlocked
            ? new Color(1.0f, 0.5f, 0.0f, 1.0f)  // Orange for blocked crit
            : new Color(1.0f, 0.85f, 0.2f, 1.0f); // Gold for unblocked crit

        var tween = vfx.CreateTween().SetParallel();
        tween.TweenProperty(vfx, "modulate", critColor, 0.5f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(vfx, "modulate:a", 0.0f, 2.0f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(vfx, "scale", Vector2.One, 1.2f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad)
            .From(Vector2.One * 2.5f);

        // Vanilla's _ExitTree kills _tween
        TweenRef(vfx) = tween;

        await tween.AwaitFinished(vfx);
        vfx.QueueFreeSafely();
    }

    /// <summary>
    /// Applies critical hit styling to the damage VFX node.
    /// Adds "!" to the damage text and tracks crit information for coloring.
    /// </summary>
    private static void ApplyCriticalHitStyle(NDamageNumVfx vfxNode, DamageResult result)
    {
        try
        {
            // Get the private _text field and append "!"
            var textField = typeof(NDamageNumVfx).GetField("_text", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (textField is not null)
            {
                string? currentText = textField.GetValue(vfxNode) as string;
                if (currentText is not null)
                {
                    string criticalText = currentText + "!";
                    textField.SetValue(vfxNode, criticalText);
                    MainFile.Logger.Info($"[CriticalDamageDisplay] Modified damage text: {currentText} → {criticalText}");
                }
            }
            else
            {
                MainFile.Logger.Warn($"[CriticalDamageDisplay] Could not find _text field");
            }

            // Store critical hit information for AnimVfx to use
            bool isBlocked = result.WasFullyBlocked;
            _criticalHitInfo[vfxNode] = (true, isBlocked);
            
            MainFile.Logger.Info($"[CriticalDamageDisplay] Stored crit info - Blocked: {isBlocked}");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[CriticalDamageDisplay] Error applying critical hit style: {ex.Message}\n{ex.StackTrace}");
        }
    }
}

