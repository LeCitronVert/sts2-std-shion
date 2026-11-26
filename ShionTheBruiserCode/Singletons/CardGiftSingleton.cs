using System.Runtime.CompilerServices;
using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Counts, for each player, how many times another player put a card into their combat piles this combat (Fae Return).
/// <para>Unlike <see cref="CardGeneratedEntry"/> alone, this also sees cards added with a plain
/// <see cref="CardPileCmd.Add(CardModel, PileType, CardPilePosition, AbstractModel?, bool)"/> (most modded cards do that)
/// and cards whose ownership was transferred (<see cref="CardPileCmd.GiveToAnotherPlayer"/>).</para>
/// <list type="bullet">
/// <item>Cards entering a player's combat pile during another player's card play count once per card play
/// (one gift, however many cards it gave).</item>
/// <item>Outside of a card play, each card generated with another player as its creator, or transferred from
/// another player, counts once.</item>
/// <item>Cards a player generated for themselves (creator = receiver) never count, even during an ally's card play.</item>
/// </list>
/// </summary>
public class CardGiftSingleton() : CustomSingletonModel(HookType.Combat)
{
    private static readonly Dictionary<Creature, int> GiftCounts = new();
    private static readonly HashSet<(CardPlay, Creature)> CountedPlays = new();

    /// <summary>Previous owner of cards being transferred by <see cref="CardPileCmd.GiveToAnotherPlayer"/>.</summary>
    private static readonly ConditionalWeakTable<CardModel, Player> PendingTransfers = new();

    public static int GiftsReceived(Creature creature) =>
        GiftCounts.GetValueOrDefault(creature);

    internal static void RegisterTransfer(CardModel card, Player previousOwner) =>
        PendingTransfers.AddOrUpdate(card, previousOwner);

    public override Task BeforeCombatStart()
    {
        Reset();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Reset();
        return Task.CompletedTask;
    }

    private static void Reset()
    {
        GiftCounts.Clear();
        CountedPlays.Clear();
        PendingTransfers.Clear();
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        // Only cards that weren't in any pile before: newly created cards, or cards changing owners
        if (oldPileType != PileType.None || card.Pile is not { IsCombatPile: true }) return Task.CompletedTask;

        var receiver = card.Owner?.Creature;
        if (receiver == null || receiver.Side != CombatSide.Player) return Task.CompletedTask;

        var entries = CombatManager.Instance.History.Entries;
        var openPlay = FindOpenPlay(entries);

        Creature? giver = null;
        if (PendingTransfers.TryGetValue(card, out var previousOwner))
        {
            PendingTransfers.Remove(card);
            giver = previousOwner.Creature;
        }
        else
        {
            var generated = entries.Reverse().OfType<CardGeneratedEntry>().FirstOrDefault(e => e.Card == card);
            if (generated?.Creator != null) giver = generated.Creator.Creature;
        }
        giver ??= openPlay?.Card.Owner?.Creature;

        if (giver == null || giver == receiver || giver.Side != CombatSide.Player) return Task.CompletedTask;

        // One gift per card play, however many cards it gave
        if (openPlay != null && openPlay.Card.Owner?.Creature == giver && !CountedPlays.Add((openPlay, receiver)))
            return Task.CompletedTask;

        GiftCounts[receiver] = GiftsReceived(receiver) + 1;
        return Task.CompletedTask;
    }

    /// <summary>The innermost card play that has started but not finished yet, if any.</summary>
    private static CardPlay? FindOpenPlay(IEnumerable<CombatHistoryEntry> entries)
    {
        HashSet<CardPlay> finished = [];
        foreach (var entry in entries.Reverse())
        {
            switch (entry)
            {
                case CardPlayFinishedEntry f:
                    finished.Add(f.CardPlay);
                    break;
                case CardPlayStartedEntry s when !finished.Contains(s.CardPlay):
                    return s.CardPlay;
            }
        }
        return null;
    }
}

[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.GiveToAnotherPlayer))]
public static class CardGiftTransferPatch
{
    [HarmonyPrefix]
    public static void Prefix(CardModel card, Player player)
    {
        if (card.Owner != null && card.Owner != player)
            CardGiftSingleton.RegisterTransfer(card, card.Owner);
    }
}
