using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Adds the Team Blast gauge (scenes/fx/team_blast_gauge.tscn) to the combat room, centered under the players.
/// It is only visible while <see cref="TeamBlastSingleton.IsActive"/>.
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom._Ready))]
public static class TeamBlastGaugePatch
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "team_blast_gauge.tscn");

    // Gap between the lowest player's health bar / powers and the gauge
    private const float VerticalGap = 12f;
    // Space kept free at the bottom of the screen
    private const float BottomMargin = 8f;
    // How fast the gauge follows the players (fraction of the distance per frame at 60 FPS)
    private const float FollowSpeed = 0.2f;

    private enum GaugeState { Filling, Ready, Locked }

    [HarmonyPostfix]
    public static void Postfix(NCombatRoom __instance)
    {
        if (__instance.Mode != CombatRoomMode.ActiveCombat) return;

        var room = __instance;
        var gauge = GD.Load<PackedScene>(ScenePath).Instantiate<Control>();
        gauge.Visible = false;
        room.AddChild(gauge);
        // Draw over the creatures, but under the combat UI (hand, energy...)
        if (room.Ui.GetParent() == room)
        {
            room.MoveChild(gauge, room.Ui.GetIndex());
        }

        var placed = false;

        void OnProcessFrame()
        {
            if (!GodotObject.IsInstanceValid(gauge)) return;

            var teamBlast = TeamBlastSingleton.Instance;
            var visible = teamBlast is { IsActive: true };
            if (gauge.Visible != visible)
            {
                gauge.Visible = visible;
                if (!visible) NHoverTipSet.Remove(gauge);
            }
            if (!visible) return;

            var state = teamBlast!.IsLocked ? GaugeState.Locked
                : teamBlast.IsFull ? GaugeState.Ready
                : GaugeState.Filling;
            gauge.Call("set_gauge", teamBlast.Value, teamBlast.MaxValue, (int)state);

            if (TryGetTargetPosition(room, gauge, out var target))
            {
                gauge.GlobalPosition = placed ? gauge.GlobalPosition.Lerp(target, FollowSpeed) : target;
                placed = true;
            }
        }

        gauge.MouseEntered += () =>
        {
            var teamBlast = TeamBlastSingleton.Instance;
            if (teamBlast is not { IsActive: true }) return;
            NHoverTipSet.CreateAndShow(gauge, teamBlast.CreateGaugeHoverTip())
                ?.SetGlobalPosition(gauge.GlobalPosition + new Vector2(gauge.Size.X + 16f, -120f));
        };
        gauge.MouseExited += () => NHoverTipSet.Remove(gauge);

        var tree = room.GetTree();
        tree.ProcessFrame += OnProcessFrame;
        gauge.TreeExiting += () => tree.ProcessFrame -= OnProcessFrame;
    }

    /// <summary>Centered on the living players, just under the lowest health bar or power row.</summary>
    private static bool TryGetTargetPosition(NCombatRoom room, Control gauge, out Vector2 target)
    {
        target = Vector2.Zero;
        var players = room.CreatureNodes
            .Where(n => GodotObject.IsInstanceValid(n) && n.Entity.IsPlayer && n.IsVisibleInTree())
            .ToList();
        if (players.Count == 0) return false;

        var centerX = players.Average(n => n.Hitbox.GetGlobalRect().GetCenter().X);
        var bottom = players.Max(PlayerBottom);

        var viewportHeight = room.GetViewportRect().Size.Y;
        var y = Math.Min(bottom + VerticalGap, viewportHeight - gauge.Size.Y - BottomMargin);
        target = new Vector2(centerX - gauge.Size.X * 0.5f, y);
        return true;
    }

    private static float PlayerBottom(NCreature creature)
    {
        var bottom = creature.Hitbox.GetGlobalRect().End.Y;
        var stateDisplay = creature.GetNodeOrNull<Control>("%HealthBar");
        if (stateDisplay == null) return bottom;

        bottom = Math.Max(bottom, stateDisplay.GetGlobalRect().End.Y);
        foreach (var name in new[] { "%HealthBar", "%PowerContainer" })
        {
            var node = stateDisplay.GetNodeOrNull<Control>(name);
            if (node is { Visible: true })
            {
                bottom = Math.Max(bottom, node.GetGlobalRect().End.Y);
            }
        }
        return bottom;
    }
}
