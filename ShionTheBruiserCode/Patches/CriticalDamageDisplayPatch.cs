using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;
using Godot;

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
    /// Postfix patch on _Ready to customize tween colors for critical hits.
    /// This runs when the VFX node is added to the scene, before AnimVfx creates the tween.
    /// </summary>
    [HarmonyPatch("_Ready", MethodType.Normal)]
    [HarmonyPostfix]
    public static void ReadyPostfix(NDamageNumVfx __instance)
    {
        try
        {
            // Check if this is a critical hit in our tracking dictionary
            if (!_criticalHitInfo.TryGetValue(__instance, out var critInfo) || !critInfo.isCrit)
                return;

            MainFile.Logger.Info($"[CriticalDamageDisplay] _Ready postfix - setting up crit colors");

            // Get the tween field that was just created
            var tweenField = typeof(NDamageNumVfx).GetField("_tween", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (tweenField is null)
                return;

            var tween = tweenField.GetValue(__instance) as Tween;
            if (tween is not null)
            {
                // Kill the existing tween with default colors
                tween.Kill();
                MainFile.Logger.Info($"[CriticalDamageDisplay] Killed default tween");
            }

            // Determine the color based on whether it's blocked
            Godot.Color critColor = critInfo.isBlocked 
                ? new Godot.Color(1.0f, 0.5f, 0.0f, 1.0f)  // Orange for blocked crit: RGB(1.0, 0.5, 0.0)
                : new Godot.Color(1.0f, 0.85f, 0.2f, 1.0f); // Gold for unblocked crit: RGB(1.0, 0.85, 0.2)

            // Create a new tween with the critical hit colors
            var newTween = __instance.CreateTween().SetParallel();
            newTween.TweenProperty(__instance, "modulate", critColor, 0.5f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            newTween.TweenProperty(__instance, "modulate:a", 0.0f, 2.0f)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Quad);
            newTween.TweenProperty(__instance, "scale", Vector2.One, 1.2f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Quad)
                .From(Vector2.One * 2.5f);

            // Update the tween field with our new tween
            tweenField.SetValue(__instance, newTween);

            MainFile.Logger.Info($"[CriticalDamageDisplay] Created custom tween - " + 
                (critInfo.isBlocked ? "Orange (Blocked)" : "Gold (Unblocked)"));
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[CriticalDamageDisplay] Error in _Ready postfix: {ex.Message}");
        }
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

