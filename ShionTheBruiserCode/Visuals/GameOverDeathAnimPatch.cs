using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Abandoning a run outside of combat kills the players with no creature node to animate. The game over screen
/// then makes new visuals for each player and plays "die" on them, but only through Spine, so our sprite
/// characters just stood there in idle. This plays their death animation through BaseLib instead.
/// Combat already plays it through StartDeathAnim, and BaseLib handles the merchant room.
/// </summary>
[HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
public static class GameOverDeathAnimPatch
{
    private static readonly AccessTools.FieldRef<NGameOverScreen, Control> CreatureContainer =
        AccessTools.FieldRefAccess<NGameOverScreen, Control>("_creatureContainer");

    [HarmonyPostfix]
    public static void Postfix(NGameOverScreen __instance)
    {
        if (NCombatRoom.Instance != null || NMerchantRoom.Instance != null) return;

        foreach (var visuals in CreatureContainer(__instance).GetChildren().OfType<NCreatureVisuals>())
        {
            if (visuals.HasSpineAnimation) continue;
            CustomAnimation.PlayCustomAnimation(visuals, "Dead", "Die", "dead", "die");
        }
    }
}
