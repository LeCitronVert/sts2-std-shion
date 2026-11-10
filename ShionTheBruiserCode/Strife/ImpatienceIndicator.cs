using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Growing Impatience indicator (scenes/fx/impatience_clock.tscn), right of the energy counter: a clock drawn in code
/// and how many cards you can still play this turn. Only shown while an enemy has <see cref="GrowingImpatience"/>.
/// Hovering it shows the power's tooltip. Visual only.
/// </summary>
[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class ImpatienceIndicatorPatch
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "impatience_clock.tscn");

    private static readonly AccessTools.FieldRef<NEnergyCounter, Player> PlayerRef =
        AccessTools.FieldRefAccess<NEnergyCounter, Player>("_player");

    private const float HorizontalGap = 8f;
    private const float PollInterval = 0.1f;

    [HarmonyPostfix]
    public static void Postfix(NEnergyCounter __instance)
    {
        var player = PlayerRef(__instance);
        if (player == null || !ResourceLoader.Exists(ScenePath)) return;

        var indicator = GD.Load<PackedScene>(ScenePath).Instantiate<Control>();
        indicator.Visible = false;
        __instance.AddChild(indicator);
        indicator.Position = new Vector2(__instance.Size.X + HorizontalGap, (__instance.Size.Y - indicator.Size.Y) * 0.5f);
        // Same font as the energy counter's number
        if (__instance.GetNodeOrNull<Label>("Label")?.GetThemeFont("font") is { } font)
        {
            indicator.GetNodeOrNull<Label>("Label")?.AddThemeFontOverride("font", font);
        }

        GrowingImpatience? Find() =>
            player.Creature.CombatState?.Enemies.Select(e => e.GetPower<GrowingImpatience>()).FirstOrDefault(p => p != null);

        void Refresh()
        {
            if (!GodotObject.IsInstanceValid(indicator)) return;
            var impatience = Find();
            indicator.Visible = impatience != null;
            if (impatience != null) indicator.Call("set_count", impatience.CardsLeft(player), impatience.Amount);
        }

        indicator.MouseEntered += () =>
        {
            if (Find() is not { } impatience) return;
            NHoverTipSet.CreateAndShow(indicator, impatience.HoverTips)
                ?.SetGlobalPosition(indicator.GlobalPosition + new Vector2(0f, -220f));
        };
        indicator.MouseExited += () => NHoverTipSet.Remove(indicator);

        // Card plays and turn starts don't all go through the combat state event: a short poll keeps it exact
        var timer = new Godot.Timer { WaitTime = PollInterval, Autostart = true };
        timer.Timeout += Refresh;
        indicator.AddChild(timer);

        void OnCombatStateChanged(CombatState _) => Refresh();
        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        indicator.TreeExiting += () => CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
        Refresh();
    }
}
