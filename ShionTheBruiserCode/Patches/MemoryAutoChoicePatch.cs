using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// While Memory auto-plays a player's cards, their choices are picked at random instead of asking the player.
/// A real choice would pause the end of turn hook and let the next ones run alongside it, which desyncs multiplayer.
/// Picks the minimum required, or 1 if the choice is optional.
/// </summary>
public static class MemoryAutoChoice
{
    private static readonly HashSet<Player> AutoChoosing = [];

    public static async Task Run(Player player, Func<Task> action)
    {
        var added = AutoChoosing.Add(player);
        try
        {
            await action();
        }
        finally
        {
            if (added) AutoChoosing.Remove(player);
        }
    }

    public static bool IsActive(Player player) => AutoChoosing.Contains(player);

    public static List<CardModel> Pick(Player player, IEnumerable<CardModel> options, int minSelect, int maxSelect)
    {
        var list = options.ToList();
        var count = Math.Min(list.Count, Math.Max(minSelect, Math.Min(maxSelect, 1)));
        return list.TakeRandom(count, player.RunState.Rng.CombatCardSelection).ToList();
    }
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromHand))]
public static class MemoryAutoChoiceFromHandPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Player player, CardSelectorPrefs prefs, Func<CardModel, bool>? filter, ref Task<IEnumerable<CardModel>> __result)
    {
        if (!MemoryAutoChoice.IsActive(player)) return true;

        var options = PileType.Hand.GetPile(player).Cards.Where(filter ?? (_ => true));
        __result = Task.FromResult<IEnumerable<CardModel>>(MemoryAutoChoice.Pick(player, options, prefs.MinSelect, prefs.MaxSelect));
        return false;
    }
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromHandForUpgrade))]
public static class MemoryAutoChoiceFromHandForUpgradePatch
{
    [HarmonyPrefix]
    public static bool Prefix(Player player, ref Task<CardModel?> __result)
    {
        if (!MemoryAutoChoice.IsActive(player)) return true;

        var options = PileType.Hand.GetPile(player).Cards.Where(c => c.IsUpgradable);
        __result = Task.FromResult(MemoryAutoChoice.Pick(player, options, 1, 1).FirstOrDefault());
        return false;
    }
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromCombatPile),
    [typeof(PlayerChoiceContext), typeof(CardPile), typeof(Player), typeof(CardSelectorPrefs), typeof(Func<CardModel, bool>)])]
public static class MemoryAutoChoiceFromCombatPilePatch
{
    [HarmonyPrefix]
    public static bool Prefix(CardPile pile, Player player, CardSelectorPrefs prefs, Func<CardModel, bool> filter, ref Task<IEnumerable<CardModel>> __result)
    {
        if (!MemoryAutoChoice.IsActive(player)) return true;

        __result = Task.FromResult<IEnumerable<CardModel>>(MemoryAutoChoice.Pick(player, pile.Cards.Where(filter), prefs.MinSelect, prefs.MaxSelect));
        return false;
    }
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGrid))]
public static class MemoryAutoChoiceFromSimpleGridPatch
{
    [HarmonyPrefix]
    public static bool Prefix(IReadOnlyList<CardModel> cardsIn, Player player, CardSelectorPrefs prefs, ref Task<IEnumerable<CardModel>> __result)
    {
        if (!MemoryAutoChoice.IsActive(player) || CombatManager.Instance.IsOverOrEnding) return true;

        __result = Task.FromResult<IEnumerable<CardModel>>(MemoryAutoChoice.Pick(player, cardsIn, prefs.MinSelect, prefs.MaxSelect));
        return false;
    }
}

[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen))]
public static class MemoryAutoChoiceFromChooseACardScreenPatch
{
    [HarmonyPrefix]
    public static bool Prefix(IReadOnlyList<CardModel> cards, Player player, ref Task<CardModel?> __result)
    {
        if (!MemoryAutoChoice.IsActive(player)) return true;

        __result = Task.FromResult(MemoryAutoChoice.Pick(player, cards, 1, 1).FirstOrDefault());
        return false;
    }
}
