using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Implemented by models that want to react to Slal's Random mechanic.
/// </summary>
public interface ISlalRandomListener
{
    /// <summary>Called whenever a creature gains Random.</summary>
    Task AfterRandomGained(PlayerChoiceContext choiceContext, Creature gainer, decimal amount) => Task.CompletedTask;

    /// <summary>Called each time a creature's Random reaches 10 (it gains 1 Memory).</summary>
    Task AfterRandomThresholdReached(PlayerChoiceContext choiceContext, Creature gainer) => Task.CompletedTask;
}

public static class SlalMechanicsHelper
{
    public static async Task TriggerNovel(PlayerChoiceContext choiceContext, CardModel card, Creature character)
    {
        card.RemoveKeyword(SlalCardKeywords.Novel);

        foreach (var copy in (character.Player?.PlayerCombatState?.AllCards ?? []).Where(c => c.Id == card.Id))
        {
            copy.RemoveKeyword(SlalCardKeywords.Novel);
        }

        await PowerCmd.Apply<Random>(
            choiceContext,
            character,
            1,
            character,
            null
        );
    }

    public static async Task EnsureCreatureHasNovelTracker(Creature creature)
    {
        if (!creature.HasPower<NovelTracker>())
        {
            await PowerCmd.Apply<NovelTracker>(
                new ThrowingPlayerChoiceContext(),
                creature,
                1,
                creature,
                null,
                true
            );
        }
    }

    public static CardModel? ChooseRandomCardWithoutNovelIfPossible(Player player, PileType pile)
    {
        var cardList = pile.GetPile(player);
        var cardWithoutNovelList = cardList.Cards.Where(c => !c.Keywords.Contains(SlalCardKeywords.Novel)).ToList();

        return cardWithoutNovelList.Count > 0
            ? player.RunState.Rng.CombatCardSelection.NextItem(cardWithoutNovelList)
            : player.RunState.Rng.CombatCardSelection.NextItem(cardList.Cards)
        ;
    }

    // ── Random ──────────────────────────────────────────────────────────────

    /// <summary>
    /// The creature's current Random, as displayed on the power (1 to 10). 0 if it has none.
    /// </summary>
    public static int GetRandom(Creature creature)
    {
        return creature.GetPower<Random>()?.GetCurrentAmount() ?? 0;
    }

    /// <summary>
    /// Number of times the creature gained Random this combat.
    /// </summary>
    public static int GetTimesRandomGained(Creature creature)
    {
        return creature.GetPower<Random>()?.TimesGained ?? 0;
    }

    public static async Task GainRandom(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source = null)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<Random>(choiceContext, creature, amount, creature, source);
    }

    public static async Task TriggerRandomGained(PlayerChoiceContext choiceContext, Creature gainer, decimal amount)
    {
        if (gainer.CombatState is null) return;
        foreach (var listener in gainer.CombatState.IterateHookListeners().OfType<ISlalRandomListener>().ToList())
        {
            await listener.AfterRandomGained(choiceContext, gainer, amount);
        }
    }

    public static async Task TriggerRandomThresholdReached(PlayerChoiceContext choiceContext, Creature gainer)
    {
        if (gainer.CombatState is null) return;
        foreach (var listener in gainer.CombatState.IterateHookListeners().OfType<ISlalRandomListener>().ToList())
        {
            await listener.AfterRandomThresholdReached(choiceContext, gainer);
        }
    }

    // ── Memory ──────────────────────────────────────────────────────────────

    public static async Task GainMemory(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source = null)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<Memory>(choiceContext, creature, amount, creature, source);
    }

    public static async Task GainMemoryThisTurn(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source = null)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<MemoryThisTurn>(choiceContext, creature, amount, creature, source);
    }

    /// <summary>
    /// Colored, non-Novel, playable cards.
    /// </summary>
    public static bool CanBePlayedByMemory(Player player, CardModel card)
    {
        return IsColored(card)
            && !card.Keywords.Contains(SlalCardKeywords.Novel)
            && !card.Keywords.Contains(CardKeyword.Unplayable);
    }

    /// <summary>
    /// Total Memory (permanent and this turn).
    /// </summary>
    public static int GetMemory(Creature creature)
    {
        return creature.GetPowerAmount<Memory>() + creature.GetPowerAmount<MemoryThisTurn>();
    }

    /// <summary>
    /// Plays up to <paramref name="count"/> cards from the Discard Pile (random, or chosen with Continuity Thread).
    /// Memory Blocked stops it this turn. Walking Encyclopedia turns it into energy and card draw next turn.
    /// </summary>
    public static async Task PlayFromMemory(PlayerChoiceContext choiceContext, Player player, int count)
    {
        if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;
        if (player.Creature.HasPower<MemoryBlocked>()) return;

        if (player.Creature.HasPower<EncyclopediaForm>())
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, player.Creature, count, player.Creature, null);
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, player.Creature, count, player.Creature, null);
            return;
        }

        var continuityThread = player.Creature.GetPower<ContinuityThread>();
        if (continuityThread != null)
        {
            var chosen = await continuityThread.SelectCardsForMemory(choiceContext, player, count, c => CanBePlayedByMemory(player, c));
            await AutoPlayFromDiscard(choiceContext, player, chosen, exhaust: true);
            return;
        }

        await AutoPlayFromDiscard(choiceContext, player, PickRandomColoredCards(player, PileType.Discard, count, c => CanBePlayedByMemory(player, c)));
    }

    /// <summary>
    /// Plays the given cards from the Discard Pile, skipping those that left it in the meantime.
    /// </summary>
    public static async Task AutoPlayFromDiscard(PlayerChoiceContext choiceContext, Player player, IEnumerable<CardModel> cards, bool exhaust = false)
    {
        foreach (var card in cards.ToList())
        {
            if (player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding) break;
            // A previous card may have moved it
            if (card.Pile?.Type != PileType.Discard) continue;

            await CardPileCmd.Add(card, PileType.Play);
            // Continuity Thread: the played card ends up Exhausted
            if (exhaust) card.ExhaustOnNextPlay = true;
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    // ── Confusion ───────────────────────────────────────────────────────────

    /// <summary>
    /// True while an effect is applying Confusion to all enemies at once (used by Pinwheel Form to avoid spreading it again).
    /// </summary>
    public static bool IsApplyingToAllEnemies { get; private set; }

    /// <summary>
    /// Applies a power to ALL hittable enemies, flagging the application as already targeting everyone.
    /// </summary>
    public static async Task ApplyToAllEnemies<T>(PlayerChoiceContext choiceContext, Creature applier, decimal amount, CardModel? source) where T : PowerModel
    {
        if (applier.CombatState is null || amount <= 0) return;

        var previous = IsApplyingToAllEnemies;
        IsApplyingToAllEnemies = true;
        try
        {
            await PowerCmd.Apply<T>(choiceContext, applier.CombatState.HittableEnemies.ToList(), amount, applier, source);
        }
        finally
        {
            IsApplyingToAllEnemies = previous;
        }
    }

    // ── Card helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// A colored card is a card that isn't Colorless, a Status or a Curse.
    /// </summary>
    public static bool IsColored(CardModel card)
    {
        return card.Type is not (CardType.Status or CardType.Curse) && !card.Pool.IsColorless;
    }

    /// <summary>
    /// Slimy or Very Slimy (Very Slimy cards count as Slimy).
    /// </summary>
    public static bool IsSlimy(CardModel card)
    {
        return card.Keywords.Contains(SlalCardKeywords.Slimy) || card.Keywords.Contains(SlalCardKeywords.VerySlimy);
    }

    /// <summary>
    /// Forms from this mod carry the Form tag; base game Forms (Demon Form, etc.) don't, so they are recognized by their name.
    /// </summary>
    public static bool IsFormPower(CardModel card)
    {
        return card.Type == CardType.Power
            && (card.Tags.Contains(SlalCardTags.Form) || card.Id.Entry.EndsWith("FORM", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The card's current energy cost (0 for X-cost cards).
    /// </summary>
    public static int GetEnergyCost(CardModel card)
    {
        return card.EnergyCost.CostsX ? 0 : Math.Max(0, card.EnergyCost.GetWithModifiers(CostModifiers.All));
    }

    public static bool IsNativeToSlal(CardModel card)
    {
        return card is SlalTheEndlessCard;
    }

    /// <summary>
    /// Picks up to <paramref name="count"/> random colored cards from the given pile.
    /// </summary>
    public static List<CardModel> PickRandomColoredCards(Player player, PileType pile, int count, Func<CardModel, bool>? filter = null)
    {
        var candidates = pile.GetPile(player).Cards
            .Where(c => IsColored(c) && (filter?.Invoke(c) ?? true))
            .ToList();

        return candidates.TakeRandom(count, player.RunState.Rng.CombatCardSelection).ToList();
    }

    /// <summary>
    /// Puts up to <paramref name="count"/> random colored cards from the Discard Pile into the Hand.
    /// </summary>
    public static async Task<List<CardModel>> PutRandomColoredCardsFromDiscardIntoHand(Player player, int count, Func<CardModel, bool>? filter = null)
    {
        var cards = PickRandomColoredCards(player, PileType.Discard, count, filter);
        foreach (var card in cards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
        return cards;
    }

    /// <summary>
    /// The cards a random card creation can pick from for this player.
    /// Normally the character's own cards. Television Form allows Colorless cards.
    /// </summary>
    public static IEnumerable<CardModel> GetRandomCardOptions(Player player)
    {
        return GetCardsFromPools(player, [player.Character.CardPool]);
    }

    /// <summary>
    /// Every character's card pool, including the modded characters.
    /// </summary>
    public static IEnumerable<CardPoolModel> AllCharacterCardPools => [
        ..ModelDb.AllCharacterCardPools,
        ModelDb.CardPool<ShionTheBruiserCardPool>(),
        ModelDb.CardPool<SlalTheEndlessCardPool>(),
        ModelDb.CardPool<SeelsTheRecklessCardPool>(),
        ModelDb.CardPool<FryTheCommanderCardPool>(),
    ];

    /// <summary>
    /// Cards from any character (or only the other characters), plus Colorless cards with Television Form.
    /// </summary>
    public static IEnumerable<CardModel> GetCharacterCardOptions(Player player, bool includeOwnCharacter)
    {
        var pools = AllCharacterCardPools.Where(p => includeOwnCharacter || p.Id != player.Character.CardPool.Id);
        return GetCardsFromPools(player, pools);
    }

    private static IEnumerable<CardModel> GetCardsFromPools(Player player, IEnumerable<CardPoolModel> pools)
    {
        List<CardPoolModel> allPools = [..pools];
        if (player.Creature.HasPower<TelevisionForm>())
        {
            allPools.Add(ModelDb.CardPool<ColorlessCardPool>());
        }

        return allPools
            .DistinctBy(p => p.Id)
            .SelectMany(p => p.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint))
            .Distinct();
    }

    /// <summary>
    /// "Random non-Rare card": Common or Uncommon, and also Rare with Creative Spirit.
    /// </summary>
    public static bool HasRandomCardRarity(Player player, CardModel card)
    {
        return card.Rarity is CardRarity.Common or CardRarity.Uncommon
            || (card.Rarity == CardRarity.Rare && player.Creature.HasPower<CreativeSpirit>());
    }

    /// <summary>
    /// Shows <paramref name="count"/> random cards from the options (skippable) and adds the chosen one into the Hand.
    /// </summary>
    public static async Task<CardModel?> ChooseCardToAddToHand(PlayerChoiceContext choiceContext, Player player, IEnumerable<CardModel> options, int count, bool upgraded)
    {
        var cards = CreateRandomCards(player, count, upgraded, options.Where(c => c.CanBeGeneratedInCombat));
        if (cards.Count == 0) return null;

        var chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, player, canSkip: true);
        if (chosen != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, player);
        }
        return chosen;
    }

    // ── Goofball Token Powers ───────────────────────────────────────────────

    public static IReadOnlyList<CardModel> GoofballTokenPowers => [
        ModelDb.Card<SlalSwordForm>(),
        ModelDb.Card<SlalOctopusForm>(),
        ModelDb.Card<SlalBoxOfNailsForm>(),
        ModelDb.Card<SlalBumperForm>(),
        ModelDb.Card<SlalVentilatorForm>(),
        ModelDb.Card<SlalSqueakySquirrelForm>(),
        ModelDb.Card<SlalWalkingEncyclopedia>(),
        ModelDb.Card<SlalTelevisionForm>(),
        ModelDb.Card<SlalSparrowForm>(),
    ];

    public static bool IsGoofballTokenPower(CardModel card)
    {
        return GoofballTokenPowers.Any(c => c.Id == card.Id);
    }

    /// <summary>
    /// Creates a random Goofball Token Power, avoiding the last one given if possible.
    /// </summary>
    public static CardModel CreateGoofballTokenPower(Player player, bool upgraded, ModelId? lastGiven = null)
    {
        return CreateTokenAvoiding(player, GoofballTokenPowers, lastGiven, upgraded);
    }

    /// <summary>
    /// Creates random cards (not added to any pile yet).
    /// </summary>
    public static List<CardModel> CreateRandomCards(Player player, int count, bool upgraded, IEnumerable<CardModel>? options = null)
    {
        var cards = CardFactory.GetDistinctForCombat(
            player,
            options ?? GetRandomCardOptions(player),
            count,
            player.RunState.Rng.CombatCardGeneration
        ).ToList();

        if (upgraded)
        {
            foreach (var card in cards.Where(c => c.IsUpgradable))
            {
                CardCmd.Upgrade(card);
            }
        }

        return cards;
    }

    /// <summary>
    /// Creates one of the given token cards, avoiding the last one that was given if possible.
    /// </summary>
    public static CardModel CreateTokenAvoiding(Player player, IReadOnlyList<CardModel> canonicalOptions, ModelId? lastGiven, bool upgraded)
    {
        var options = canonicalOptions.Where(c => c.Id != lastGiven).ToList();
        if (options.Count == 0) options = canonicalOptions.ToList();

        var chosen = player.RunState.Rng.CombatCardGeneration.NextItem(options)!;
        var card = player.Creature.CombatState!.CreateCard(chosen, player);
        if (upgraded && card.IsUpgradable)
        {
            CardCmd.Upgrade(card);
        }
        return card;
    }

    /// <summary>
    /// Creates a card of type T, upgraded if needed.
    /// </summary>
    public static CardModel CreateCard<T>(Player player, bool upgraded = false) where T : CardModel
    {
        var card = player.Creature.CombatState!.CreateCard<T>(player);
        if (upgraded && card.IsUpgradable)
        {
            CardCmd.Upgrade(card);
        }
        return card;
    }

    /// <summary>
    /// Steady: the card gains Retain.
    /// </summary>
    public static void MakeSteady(CardModel card)
    {
        CardCmd.Enchant<Steady>(card, 1);
    }

    // ── Multiplayer ─────────────────────────────────────────────────────────

    public static string GetPlayerName(Player player)
    {
        return PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId);
    }

    /// <summary>
    /// Creates a copy of the card for another player, at the same upgrade level (plus one if <paramref name="upgradeOnce"/>).
    /// Enchantments and in-combat changes aren't copied.
    /// </summary>
    public static CardModel CreateCopyForPlayer(CardModel card, Player receiver, bool upgradeOnce = false)
    {
        var copy = receiver.Creature.CombatState!.CreateCard(card.CanonicalInstance, receiver);
        var upgrades = card.CurrentUpgradeLevel + (upgradeOnce ? 1 : 0);
        while (copy.CurrentUpgradeLevel < upgrades && copy.IsUpgradable)
        {
            CardCmd.Upgrade(copy);
        }
        return copy;
    }

    /// <summary>
    /// "Puts" a card into another player's pile. Cards can't change owner, so the receiver gets a copy
    /// and the original leaves combat (the deck isn't affected).
    /// The copy isn't counted as a card the receiver created. Giving a card to its own owner just moves it.
    /// </summary>
    public static async Task GiveCardToPlayer(CardModel card, Player receiver, PileType pile, bool upgradeOnce = false)
    {
        if (card.Owner == receiver)
        {
            if (upgradeOnce && card.IsUpgradable) CardCmd.Upgrade(card);
            await CardPileCmd.Add(card, pile);
            return;
        }

        var copy = CreateCopyForPlayer(card, receiver, upgradeOnce);
        await CardPileCmd.RemoveFromCombat(card);
        await CardPileCmd.AddGeneratedCardToCombat(copy, pile, null);
    }
}
