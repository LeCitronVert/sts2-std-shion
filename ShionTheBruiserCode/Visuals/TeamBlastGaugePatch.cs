using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Adds the Team Blast gauge (scenes/fx/team_blast_gauge.tscn) under the relic list.
/// It is only visible in combat, while <see cref="TeamBlastSingleton.IsActive"/>.
/// </summary>
[HarmonyPatch(typeof(NGlobalUi), nameof(NGlobalUi._Ready))]
public static class TeamBlastGaugePatch
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "team_blast_gauge.tscn");

    // Gap between the relic list's last row and the gauge
    private const float VerticalGap = 8f;
    // Horizontal offset from the relic list's left edge
    private const float LeftOffset = 12f;
    // Height of one relic row (see NRelicInventory.HideImmediately)
    private const float RelicRowHeight = 68f;
    // With more relic rows than this, the gauge stops going down and overlaps the relics instead
    private const int MaxRowsAbove = 4;

    private enum GaugeState { Filling, Ready, Locked }

    [HarmonyPostfix]
    public static void Postfix(NGlobalUi __instance)
    {
        var relics = __instance.RelicInventory;
        var gauge = GD.Load<PackedScene>(ScenePath).Instantiate<Control>();
        gauge.Visible = false;
        // Positioned by hand, even if the relic list's parent is a container
        gauge.TopLevel = true;
        // Sibling right after the relic list, so it draws over it when there are too many relics
        relics.AddSibling(gauge);

        void OnProcessFrame()
        {
            if (!GodotObject.IsInstanceValid(gauge) || !GodotObject.IsInstanceValid(relics)) return;

            var teamBlast = TeamBlastSingleton.Instance;
            var visible = teamBlast is { IsActive: true } && CombatManager.Instance.IsInProgress;
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

            gauge.GlobalPosition = TargetPosition(relics);
        }

        gauge.MouseEntered += () =>
        {
            var teamBlast = TeamBlastSingleton.Instance;
            if (teamBlast is not { IsActive: true }) return;
            NHoverTipSet.CreateAndShow(gauge, teamBlast.CreateGaugeHoverTip())
                ?.SetGlobalPosition(gauge.GlobalPosition + new Vector2(0f, gauge.Size.Y + 8f));
        };
        gauge.MouseExited += () => NHoverTipSet.Remove(gauge);

        var tree = __instance.GetTree();
        tree.ProcessFrame += OnProcessFrame;
        gauge.TreeExiting += () => tree.ProcessFrame -= OnProcessFrame;
    }

    /// <summary>
    /// Under the relic list's last row, but never more than <see cref="MaxRowsAbove"/> rows down.
    /// Follows the relic list when it slides in and out.
    /// </summary>
    private static Vector2 TargetPosition(NRelicInventory relics)
    {
        var rect = relics.GetGlobalRect();
        var scale = relics.GetGlobalTransform().Scale.Y;
        var maxBottom = rect.Position.Y + MaxRowsAbove * RelicRowHeight * scale;
        var bottom = Math.Min(rect.End.Y, maxBottom);
        return new Vector2(rect.Position.X + LeftOffset, bottom + VerticalGap);
    }
}
