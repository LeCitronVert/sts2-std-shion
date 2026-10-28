using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Implemented by models that want to react to Seels' mechanics.
/// </summary>
public interface ISeelsListener
{
    /// <summary>Called after a player Blood Bends (consumes ALL their Blood), with X the amount consumed.</summary>
    Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source) => Task.CompletedTask;

    /// <summary>Called after a player Gambles X.</summary>
    Task AfterGamble(PlayerChoiceContext choiceContext, Player player, int amount, IReadOnlyList<CardModel> cards) => Task.CompletedTask;

    /// <summary>Called after an Erratic card is drawn and its cost randomized.</summary>
    Task AfterErraticCostRandomized(PlayerChoiceContext choiceContext, CardModel card, int cost) => Task.CompletedTask;
}

public static class SeelsMechanicsHelper
{
    public const int BaseErraticMaxCost = 3;

    public static LocString GamblePrompt => new("card_selection", "SHIONTHEBRUISER-TO_GAMBLE");

    // ── Blood ───────────────────────────────────────────────────────────────

    public static int GetBlood(Creature creature)
    {
        return creature.GetPowerAmount<Blood>();
    }

    public static async Task GainBlood(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source = null)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<Blood>(choiceContext, creature, amount, creature, source);
    }

    /// <summary>
    /// Blood Bend X: consumes ALL your Blood, heals that much and returns the amount consumed (X).
    /// The heal only counts the Blood consumed, not the (X+1) bonuses the cards add on top.
    /// </summary>
    public static async Task<int> BloodBend(PlayerChoiceContext choiceContext, CardModel card)
    {
        var creature = card.Owner.Creature;
        var x = GetBlood(creature);
        await PowerCmd.Remove<Blood>(creature);
        if (x > 0)
        {
            await CreatureCmd.Heal(creature, x);
        }

        if (creature.CombatState is not null)
        {
            foreach (var listener in creature.CombatState.IterateHookListeners().OfType<ISeelsListener>().ToList())
            {
                await listener.AfterBloodBend(choiceContext, card.Owner, x, card);
            }
        }

        return x;
    }

    // ── Gamble / Erratic ────────────────────────────────────────────────────

    /// <summary>
    /// Gamble X: choose X cards in your Hand, add Erratic to them this combat and put them on top of your Draw Pile.
    /// </summary>
    public static async Task<IReadOnlyList<CardModel>> Gamble(PlayerChoiceContext choiceContext, Player player, int amount, AbstractModel source)
    {
        var hand = PileType.Hand.GetPile(player).Cards.Where(c => c != source).ToList();
        var count = Math.Min(amount, hand.Count);
        if (count <= 0) return [];

        var cards = (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(GamblePrompt, count),
            c => c != source,
            source
        )).ToList();

        foreach (var card in cards)
        {
            AddErratic(card);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
        }

        if (player.Creature.CombatState is not null)
        {
            foreach (var listener in player.Creature.CombatState.IterateHookListeners().OfType<ISeelsListener>().ToList())
            {
                await listener.AfterGamble(choiceContext, player, amount, cards);
            }
        }

        return cards;
    }

    /// <summary>
    /// Gamble for another player: the cards are picked at random from their Hand.
    /// </summary>
    public static async Task<IReadOnlyList<CardModel>> GambleRandomly(PlayerChoiceContext choiceContext, Player player, int amount)
    {
        var cards = PileType.Hand.GetPile(player).Cards.ToList()
            .TakeRandom(amount, player.RunState.Rng.CombatCardSelection).ToList();

        foreach (var card in cards)
        {
            AddErratic(card);
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
        }

        if (player.Creature.CombatState is not null)
        {
            foreach (var listener in player.Creature.CombatState.IterateHookListeners().OfType<ISeelsListener>().ToList())
            {
                await listener.AfterGamble(choiceContext, player, amount, cards);
            }
        }

        return cards;
    }

    public static bool IsErratic(CardModel card) => card.Keywords.Contains(SeelsCardKeywords.Erratic);

    /// <summary>
    /// Adds Erratic to a card (removing Unplayable) and makes sure its owner can randomize its cost when drawn.
    /// </summary>
    public static void AddErratic(CardModel card)
    {
        if (!IsErratic(card))
        {
            card.AddKeyword(SeelsCardKeywords.Erratic);
            CountModification(card);
        }
        card.RemoveKeyword(CardKeyword.Unplayable);
        EnsureCreatureHasSeelsTracker(card.Owner.Creature);
    }

    public static int GetErraticMaxCost(Player player)
    {
        return BaseErraticMaxCost + player.Creature.GetPowerAmount<BigNumbers>();
    }

    public static async Task RandomizeErraticCost(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.EnergyCost.CostsX) return;

        var cost = card.Owner.RunState.Rng.CombatEnergyCosts.NextInt(0, GetErraticMaxCost(card.Owner) + 1);
        card.EnergyCost.SetCustomBaseCost(cost);

        if (card.CombatState is null) return;
        foreach (var listener in card.CombatState.IterateHookListeners().OfType<ISeelsListener>().ToList())
        {
            await listener.AfterErraticCostRandomized(choiceContext, card, cost);
        }
    }

    // ── Tracker ─────────────────────────────────────────────────────────────

    public static void EnsureCreatureHasSeelsTracker(Creature creature)
    {
        if (creature.HasPower<SeelsTracker>()) return;

        PowerCmd.Apply<SeelsTracker>(
            new ThrowingPlayerChoiceContext(),
            creature,
            1,
            creature,
            null,
            true
        );
    }

    /// <summary>
    /// Counts a card modified with a Keyword or an Enchantment this combat (Magical Overload).
    /// </summary>
    public static void CountModification(CardModel card)
    {
        if (card.CombatState is null) return;
        EnsureCreatureHasSeelsTracker(card.Owner.Creature);
        card.Owner.Creature.GetPower<SeelsTracker>()?.CountModification(card);
    }

    public static int GetModifiedCardsCount(Creature creature)
    {
        return creature.GetPower<SeelsTracker>()?.ModifiedCards.Count ?? 0;
    }

    // ── Wounds ──────────────────────────────────────────────────────────────

    public static bool IsWound(CardModel card) => card is Wound;

    public static async Task AddWounds(Player player, int amount, PileType pile = PileType.Hand)
    {
        if (amount <= 0 || player.Creature.CombatState is null) return;
        var wounds = Enumerable.Range(0, amount).Select(_ => (CardModel)player.Creature.CombatState.CreateCard<Wound>(player)).ToList();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(wounds, pile, player));
    }

    public static IEnumerable<CardModel> GetWoundsInHand(Player player)
    {
        return PileType.Hand.GetPile(player).Cards.Where(IsWound).ToList();
    }

    /// <summary>
    /// Enchants a card and counts it as modified (Magical Overload).
    /// </summary>
    public static void Enchant<T>(CardModel card, decimal amount) where T : EnchantmentModel
    {
        CardCmd.Enchant<T>(card, amount);
        CountModification(card);
    }

    /// <summary>
    /// Enchants a card even if it is a Status (Wounds), which the base game normally refuses.
    /// If the card already has another enchantment, nothing happens.
    /// </summary>
    public static void ForceEnchant<T>(CardModel card, decimal amount) where T : EnchantmentModel
    {
        if (card.Enchantment is not null && card.Enchantment is not T) return;

        if (card.Enchantment is null)
        {
            var enchantment = ModelDb.Enchantment<T>().ToMutable();
            card.EnchantInternal(enchantment, amount);
            enchantment.ModifyCard();
        }
        else
        {
            card.Enchantment.Amount += (int)amount;
        }
        card.FinalizeUpgradeInternal();
        CountModification(card);
    }

    /// <summary>
    /// The Wounds in your Hand lose Unplayable and are enchanted with T.
    /// </summary>
    public static void EmpowerWoundsInHand<T>(Player player, decimal amount) where T : EnchantmentModel
    {
        foreach (var wound in GetWoundsInHand(player))
        {
            wound.RemoveKeyword(CardKeyword.Unplayable);
            ForceEnchant<T>(wound, amount);
        }
    }

    public static IEnumerable<CardModel> GetAllCombatCards(Player player)
    {
        return player.PlayerCombatState?.AllCards ?? [];
    }

    // ── Slashes ─────────────────────────────────────────────────────────────

    public static bool IsSlash(CardModel card) => card is SeelsSlash;

    public static CardModel CreateSlash(Player player, bool upgraded)
    {
        var slash = player.Creature.CombatState!.CreateCard<SeelsSlash>(player);
        if (upgraded)
        {
            CardCmd.Upgrade(slash);
        }
        return slash;
    }

    public static async Task AddSlashes(Player player, int amount, PileType pile, bool upgraded, CardPilePosition position = CardPilePosition.Random)
    {
        if (amount <= 0) return;
        var slashes = Enumerable.Range(0, amount).Select(_ => CreateSlash(player, upgraded)).ToList();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(slashes, pile, player, position));
    }

    // ── Misc ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lose HP (unblockable, unpowered), like Bloodletting.
    /// </summary>
    public static async Task LoseHp(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if (amount <= 0) return;
        await CreatureCmd.Damage(choiceContext, creature, amount, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, creature, source);
    }

    public static int GetVulnerable(Creature creature)
    {
        return creature.GetPowerAmount<MegaCrit.Sts2.Core.Models.Powers.VulnerablePower>();
    }

    public static bool IntendsToAttack(Creature? enemy)
    {
        return enemy?.Monster?.IntendsToAttack ?? false;
    }
}
