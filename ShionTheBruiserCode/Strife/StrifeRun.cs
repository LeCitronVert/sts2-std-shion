using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>Access to RunManager internals the Act 4 flow needs.</summary>
public static class StrifeRun
{
    private static readonly Func<RunManager, RunState?> StateGetter =
        AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

    private static readonly Action<RunState, IReadOnlyList<MegaCrit.Sts2.Core.Models.ActModel>> ActsSetter =
        AccessTools.MethodDelegate<Action<RunState, IReadOnlyList<MegaCrit.Sts2.Core.Models.ActModel>>>(
            AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Acts)));

    private static readonly Func<RunManager, bool, Task> FadeInMethod =
        AccessTools.MethodDelegate<Func<RunManager, bool, Task>>(AccessTools.Method(typeof(RunManager), "FadeIn"));

    private static readonly Action<RunManager> ClearScreensMethod =
        AccessTools.MethodDelegate<Action<RunManager>>(AccessTools.Method(typeof(RunManager), "ClearScreens"));

    public static RunState? State => StateGetter(RunManager.Instance);

    public static void SetActs(RunState state, IReadOnlyList<MegaCrit.Sts2.Core.Models.ActModel> acts) => ActsSetter(state, acts);

    public static Task FadeIn(bool showTransition = true) => FadeInMethod(RunManager.Instance, showTransition);

    public static void ClearScreens() => ClearScreensMethod(RunManager.Instance);

    /// <summary>
    /// Act 4 is open when every player holds a combo "Chaos Emeralds" relic. Those are only granted by Seek when the
    /// player had all seven Emeralds unlocked in their save, so this covers both conditions, and it reads only run
    /// state (the same on every client, and kept in the run save).
    /// </summary>
    public static bool IsStrifeUnlocked(RunState state) => state.Players.All(HoldsComboEmerald);

    public static bool HoldsComboEmerald(Player player) =>
        player.Relics.Any(r => r is ChaosEmeraldRelic { IsCombo: true });
}
