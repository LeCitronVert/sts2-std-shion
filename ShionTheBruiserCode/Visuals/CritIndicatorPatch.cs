using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Adds the Crit Hit Rate indicator (scenes/fx/crit_indicator.tscn) above the energy counter of any character.
/// It is only visible while the player has <see cref="ShionCritTrackerPower"/>, and replaces that power's buff icon.
/// </summary>
[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class CritIndicatorPatch
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "crit_indicator.tscn");

    private static readonly AccessTools.FieldRef<NEnergyCounter, Player> PlayerRef =
        AccessTools.FieldRefAccess<NEnergyCounter, Player>("_player");

    // Gap between the indicator's bottom and the energy counter's top
    private const float VerticalGap = 6f;

    [HarmonyPostfix]
    public static void Postfix(NEnergyCounter __instance)
    {
        var player = PlayerRef(__instance);
        if (player == null) return;

        var indicator = GD.Load<PackedScene>(ScenePath).Instantiate<Control>();
        indicator.Visible = false;
        __instance.AddChild(indicator);
        // The indicator keeps itself centered on anchor_x whenever its number of digits changes
        var centerX = __instance.Size.X * 0.5f;
        indicator.Set("anchor_x", centerX);
        indicator.Position = new Vector2(centerX - indicator.Size.X * 0.5f, -indicator.Size.Y - VerticalGap);

        void Refresh()
        {
            if (!GodotObject.IsInstanceValid(indicator)) return;
            var tracker = player.Creature.GetPower<ShionCritTrackerPower>();
            indicator.Visible = tracker != null;
            if (tracker != null)
            {
                indicator.Call("set_rate", tracker.HitRatePercent);
            }
        }

        void OnCombatStateChanged(CombatState _) => Refresh();

        indicator.MouseEntered += () =>
        {
            var tracker = player.Creature.GetPower<ShionCritTrackerPower>();
            if (tracker == null) return;
            NHoverTipSet.CreateAndShow(indicator, tracker.CreateIndicatorHoverTip())
                ?.SetGlobalPosition(indicator.GlobalPosition + new Vector2(-70f, -200f));
        };
        indicator.MouseExited += () => NHoverTipSet.Remove(indicator);

        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        indicator.TreeExiting += () => CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
        Refresh();
    }
}
