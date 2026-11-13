using ShionTheBruiser.ShionTheBruiserCode.Helper;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer). Whenever you gain or lose Max HP, ALL other players also gain or lose the same amount.
/// The mirrored changes don't trigger Sailing Ships again, so two players holding one can't loop.
/// </summary>
public class SailingShip : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    private static readonly AsyncLocal<bool> Mirroring = new();

    private static bool Holds(Creature creature) =>
        creature.Player?.Relics.Any(r => r is SailingShip) == true;

    internal static async Task MirrorGain(Task original, Creature creature, decimal amount)
    {
        await original;
        if (Mirroring.Value || amount <= 0 || !Holds(creature)) return;

        Mirroring.Value = true;
        try
        {
            creature.Player!.Relics.OfType<SailingShip>().First().Flash();
            foreach (var other in creature.Player.RunState.Players.Where(p => p != creature.Player && !p.Creature.IsDead))
            {
                await CreatureCmd.GainMaxHp(other.Creature, amount);
            }
        }
        finally
        {
            Mirroring.Value = false;
        }
    }

    internal static async Task MirrorLoss(Task original, PlayerChoiceContext choiceContext, Creature creature, decimal amount)
    {
        await original;
        if (Mirroring.Value || amount <= 0 || !Holds(creature)) return;

        Mirroring.Value = true;
        try
        {
            creature.Player!.Relics.OfType<SailingShip>().First().Flash();
            foreach (var other in creature.Player.RunState.Players.Where(p => p != creature.Player && !p.Creature.IsDead))
            {
                await Sts2Compat.LoseMaxHp(choiceContext, other.Creature, amount, null);
            }
        }
        finally
        {
            Mirroring.Value = false;
        }
    }
}

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainMaxHp))]
public static class SailingShipGainPatch
{
    [HarmonyPostfix]
    public static void Postfix(Creature creature, decimal amount, ref Task __result) =>
        __result = SailingShip.MirrorGain(__result, creature, amount);
}

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.LoseMaxHp))]
public static class SailingShipLossPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerChoiceContext choiceContext, Creature creature, decimal amount, ref Task __result) =>
        __result = SailingShip.MirrorLoss(__result, choiceContext, creature, amount);
}
