using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Notifies <see cref="ICardAfflictionListener"/>s when a card gets a new affliction.
/// Hooks CardCmd.Afflict (which callers await) rather than the synchronous AfflictionModel.AfterApplied,
/// so listeners' commands are chained onto the returned Task instead of being fired and forgotten,
/// which would let them resolve at a different time on each client and desync multiplayer.
/// </summary>
[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Afflict), [typeof(AfflictionModel), typeof(CardModel), typeof(decimal)])]
public class AfflictionHooks
{
    [HarmonyPrefix]
    public static void Prefix(CardModel card, out bool __state)
    {
        // Only a brand new affliction counts, not stacking onto an existing one
        __state = card.Affliction == null;
    }

    [HarmonyPostfix]
    public static void Postfix(CardModel card, bool __state, ref Task<AfflictionModel?> __result)
    {
        if (!__state) return;
        __result = NotifyListeners(__result, card);
    }

    private static async Task<AfflictionModel?> NotifyListeners(Task<AfflictionModel?> afflict, CardModel card)
    {
        var affliction = await afflict;
        if (affliction == null || card.Affliction != affliction) return affliction;

        foreach (var listener in card.CombatState?.IterateHookListeners().OfType<ICardAfflictionListener>().ToList() ?? [])
        {
            await listener.OnAfflictionApplied(affliction, card);
        }

        return affliction;
    }
}
