using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Shion's Ancient relics. When the player is Shion, each one can take the place of one of its Ancient's options,
/// as often as the other relics it competes with. Each one is also listed in its Ancient's AllPossibleOptions,
/// so the "ancient" console command and the Relic Collection know about it.
/// </summary>
public static class ShionAncientRelics
{
    private static readonly MethodInfo RelicOption = AccessTools.Method(typeof(AncientEventModel), "RelicOption",
        [typeof(RelicModel), typeof(string), typeof(string)]);

    public static bool IsShion(Player? player) => player?.Character is Character.ShionTheBruiser;

    public static EventOption Option(AncientEventModel ancient, RelicModel relic, string? customDonePage = null,
        Func<EventOption, EventOption>? configure = null)
    {
        var option = (EventOption)RelicOption.Invoke(ancient, [relic.ToMutable(), "INITIAL", customDonePage])!;
        return configure?.Invoke(option) ?? option;
    }

    /// <summary>
    /// Replaces the option at <paramref name="index"/> with the relic, 1 time out of <paramref name="odds"/>.
    /// </summary>
    public static void TryReplaceOption(AncientEventModel ancient, ref IReadOnlyList<EventOption> options, int index,
        int odds, RelicModel relic, string? customDonePage = null, Func<EventOption, EventOption>? configure = null)
    {
        if (index >= options.Count) return;
        if (ancient.Rng.NextInt(odds) != 0) return;

        var list = options.ToList();
        list[index] = Option(ancient, relic, customDonePage, configure);
        options = list;
    }
}

/// <summary>
/// Vakuu: Thorn Ring joins his first pool (Blood-Soaked Rose, Whispering Earring, Fiddle), so 1 in 4.
/// </summary>
[HarmonyPatch(typeof(Vakuu), "GenerateInitialOptions")]
public static class ThornRingVakuuPatch
{
    [HarmonyPostfix]
    public static void Postfix(Vakuu __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (!ShionAncientRelics.IsShion(__instance.Owner)) return;
        ShionAncientRelics.TryReplaceOption(__instance, ref __result, 0, 4, ModelDb.Relic<ThornRing>());
    }
}

[HarmonyPatch(typeof(Vakuu), nameof(AncientEventModel.AllPossibleOptions), MethodType.Getter)]
public static class ThornRingVakuuOptionsPatch
{
    [HarmonyPostfix]
    public static void Postfix(Vakuu __instance, ref IEnumerable<EventOption> __result)
    {
        __result = __result.Append(ShionAncientRelics.Option(__instance, ModelDb.Relic<ThornRing>()));
    }
}

/// <summary>
/// Neow: Turtle Hermit Shell can take her second blessing, 1 in 8 like a regular blessing (2 shown out of ~16).
/// The first one is left to Friendship Bracelet (FriendshipBraceletNeowPatch).
/// </summary>
[HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
public static class TurtleHermitShellNeowPatch
{
    [HarmonyPostfix]
    public static void Postfix(Neow __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance.Owner is not { } player || !ShionAncientRelics.IsShion(player)) return;
        if (player.RunState.Modifiers.Count > 0) return;
        if (!ModelDb.Relic<TurtleHermitShell>().IsAllowedAtNeow(player)) return;

        ShionAncientRelics.TryReplaceOption(__instance, ref __result, 1, 8, ModelDb.Relic<TurtleHermitShell>(),
            "NEOW.pages.DONE.POSITIVE.description");
    }
}

/// <summary>
/// Also read by Neow's Bones, which TurtleHermitShell.IsAllowedAtNeow keeps to Shion.
/// </summary>
[HarmonyPatch(typeof(Neow), nameof(AncientEventModel.AllPossibleOptions), MethodType.Getter)]
public static class TurtleHermitShellNeowOptionsPatch
{
    [HarmonyPostfix]
    public static void Postfix(Neow __instance, ref IEnumerable<EventOption> __result)
    {
        __result = __result.Append(ShionAncientRelics.Option(__instance, ModelDb.Relic<TurtleHermitShell>(),
            "NEOW.pages.DONE.POSITIVE.description"));
    }
}

/// <summary>
/// Orobas: Master Ball joins his second pool (Alchemical Coffer, Driftwood, Radiant Pearl), so 1 in 4,
/// once Abby the Absol is unlocked (Shion's Determination epoch).
/// </summary>
[HarmonyPatch(typeof(Orobas), "GenerateInitialOptions")]
public static class MasterBallOrobasPatch
{
    [HarmonyPostfix]
    public static void Postfix(Orobas __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance.Owner is not { } player || !ShionAncientRelics.IsShion(player)) return;
        if (!EpochLocks.Filter(player.UnlockState, [ModelDb.Relic<AbbyTheAbsol>()]).Any()) return;

        var maxHpLoss = ModelDb.Relic<MasterBall>().DynamicVars.HpLoss.BaseValue;
        ShionAncientRelics.TryReplaceOption(__instance, ref __result, 1, 4, ModelDb.Relic<MasterBall>(),
            configure: option => option.ThatDecreasesMaxHp(maxHpLoss));
    }
}

[HarmonyPatch(typeof(Orobas), nameof(AncientEventModel.AllPossibleOptions), MethodType.Getter)]
public static class MasterBallOrobasOptionsPatch
{
    [HarmonyPostfix]
    public static void Postfix(Orobas __instance, ref IEnumerable<EventOption> __result)
    {
        var maxHpLoss = ModelDb.Relic<MasterBall>().DynamicVars.HpLoss.BaseValue;
        __result = __result.Append(ShionAncientRelics.Option(__instance, ModelDb.Relic<MasterBall>(),
            configure: option => option.ThatDecreasesMaxHp(maxHpLoss)));
    }
}
