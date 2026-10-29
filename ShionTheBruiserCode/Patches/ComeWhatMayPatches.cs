using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Come What May: you can't heal during this fight.
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Heal))]
public static class ComeWhatMayHealPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Creature creature, ref Task __result)
    {
        if (!creature.HasPower<ComeWhatMay>()) return true;

        __result = Task.CompletedTask;
        return false;
    }
}

/// <summary>
/// Come What May: you can't revive during this fight. Death preventers (Lizard Tail, Fairy in a Bottle...) are ignored.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ShouldDie))]
public static class ComeWhatMayDeathPatch
{
    [HarmonyPostfix]
    public static void Postfix(Creature creature, ref bool __result, ref AbstractModel? preventer)
    {
        if (__result || !creature.HasPower<ComeWhatMay>()) return;

        __result = true;
        preventer = null;
    }
}
