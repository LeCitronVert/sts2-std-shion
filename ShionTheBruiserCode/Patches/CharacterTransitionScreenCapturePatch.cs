using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Our character select transitions (Character/*.cs → CustomCharacterSelectTransitionPath) distort the screen they cover,
/// so their shaders read it through hint_screen_texture. Godot only copies the screen for the FIRST screen-reading item
/// of a frame and every later one reuses that copy; the menus' blurred backdrops read it first, so without this the
/// transition only saw the backdrop, not the UI on top of it.
/// A BackBufferCopy right before the transition rect forces a fresh copy of the full current screen. It is only visible
/// while one of our transitions is fading out, so it costs nothing the rest of the time.
/// </summary>
[HarmonyPatch]
public static class CharacterTransitionScreenCapturePatch
{
    private const string BackBufferName = "ShionTheBruiserTransitionBackBuffer";
    private static readonly string OurTransitionsPrefix = $"{MainFile.ResPath}/materials/transitions/";

    [HarmonyPatch(typeof(NTransition), nameof(NTransition.FadeOut))]
    [HarmonyPrefix]
    public static void BeforeFadeOut(NTransition __instance, string transitionPath)
    {
        bool ours = transitionPath.StartsWith(OurTransitionsPrefix, StringComparison.Ordinal);
        var backBuffer = ours ? GetOrCreate(__instance) : Find(__instance);
        if (backBuffer != null) backBuffer.Visible = ours;
    }

    [HarmonyPatch(typeof(NTransition), nameof(NTransition.FadeIn))]
    [HarmonyPrefix]
    public static void BeforeFadeIn(NTransition __instance)
    {
        var backBuffer = Find(__instance);
        if (backBuffer != null) backBuffer.Visible = false;
    }

    private static BackBufferCopy? Find(NTransition transition) =>
        transition.GetParent()?.GetNodeOrNull<BackBufferCopy>(BackBufferName);

    private static BackBufferCopy? GetOrCreate(NTransition transition)
    {
        var parent = transition.GetParent();
        if (parent == null) return null;

        var backBuffer = parent.GetNodeOrNull<BackBufferCopy>(BackBufferName);
        if (backBuffer == null)
        {
            backBuffer = new BackBufferCopy
            {
                Name = BackBufferName,
                CopyMode = BackBufferCopy.CopyModeEnum.Viewport,
                // Same draw layer as the transition rect, and placed right before it, so it copies everything under it
                ZIndex = transition.ZIndex,
                ZAsRelative = transition.ZAsRelative,
                Visible = false,
            };
            parent.AddChild(backBuffer);
        }

        int target = transition.GetIndex();
        // MoveChild takes the final index: coming from before the rect, the rect shifts down by one once we leave
        if (backBuffer.GetIndex() < target) target--;
        if (backBuffer.GetIndex() != target) parent.MoveChild(backBuffer, target);

        return backBuffer;
    }
}
