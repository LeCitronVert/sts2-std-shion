using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Adds a "Chaos Emeralds" button under Compendium in the main menu, opening <see cref="ChaosEmeraldsScreen"/>.
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class ChaosEmeraldsMainMenuPatch
{
    private const string ButtonName = "ShionTheBruiserChaosEmeraldsButton";

    public static void Postfix(NMainMenu __instance)
    {
        try
        {
            var compendium = __instance.GetNode<NMainMenuTextButton>("MainMenuTextButtons/CompendiumButton");
            var container = compendium.GetParent();
            if (container.HasNode(ButtonName)) return;

            var button = (NMainMenuTextButton)compendium.Duplicate();
            button.Name = ButtonName;
            container.AddChild(button);
            container.MoveChild(button, compendium.GetIndex() + 1);
            button.SetLocalization("SHIONTHEBRUISER-CHAOS_EMERALDS");
            button.Visible = true;
            // Duplicate() copies the Compendium's dimmed label/focus state when the game has it disabled
            // (undiscovered epochs), but not the C# _isEnabled flag, so a bare Enable() would no-op.
            // Toggle off→on to force OnEnable to restore the label colour and focus mode.
            button.Disable();
            button.Enable();

            var focused = AccessTools.Method(typeof(NMainMenu), "MainMenuButtonFocused");
            var unfocused = AccessTools.Method(typeof(NMainMenu), "MainMenuButtonUnfocused");
            button.Connect(NClickableControl.SignalName.Focused, Callable.From((NMainMenuTextButton b) =>
                Callable.From(() => focused?.Invoke(__instance, [b])).CallDeferred()));
            button.Connect(NClickableControl.SignalName.Unfocused, Callable.From((NMainMenuTextButton b) =>
                unfocused?.Invoke(__instance, [b])));

            button.Connect(NClickableControl.SignalName.Released, Callable.From((NButton _) =>
            {
                var font = button.label?.GetThemeFont("font");
                __instance.AddChild(ChaosEmeraldsScreen.Create(font));
            }));
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not add the Chaos Emeralds main menu button: {ex}");
        }
    }
}
