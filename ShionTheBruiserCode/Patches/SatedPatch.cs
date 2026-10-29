using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Creatures with <see cref="Sated"/> can't gain Max HP.
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainMaxHp))]
public static class SatedPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Creature creature, decimal amount, ref Task __result)
    {
        if (amount <= 0 || !creature.HasPower<Sated>()) return true;

        __result = Task.CompletedTask;
        return false;
    }
}
