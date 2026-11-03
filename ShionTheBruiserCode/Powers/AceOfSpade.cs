using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Shows (and grants as real Strength and Dexterity) the bonus from the Ace of Spade cards in your Hand:
/// its Amount is always the total energy cost of those cards.
/// Whenever that total changes, the difference is applied to Strength, Dexterity and this power,
/// so the Strength and Dexterity icons show the bonus, and it's removed once no Ace is left in Hand.
/// </summary>
public class AceOfSpade : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    /// <summary>The bonus the Aces currently in the creature's Hand should grant.</summary>
    private static int TargetBonus(Creature creature)
    {
        if (creature.Player is not { } player) return 0;
        return PileType.Hand.GetPile(player).Cards
            .OfType<SeelsAceOfSpade>()
            .Sum(card => Math.Max(0, card.EnergyCost.GetResolved()));
    }

    /// <summary>
    /// Brings this power, Strength and Dexterity in line with the Aces in Hand.
    /// Idempotent, so every hook that could change the total can call it.
    /// </summary>
    public static async Task Sync(PlayerChoiceContext choiceContext, Creature creature)
    {
        if (creature.CombatState is null || creature.IsDead) return;

        var current = creature.GetPower<AceOfSpade>()?.Amount ?? 0;
        var diff = TargetBonus(creature) - current;
        if (diff == 0) return;

        await PowerCmd.Apply<AceOfSpade>(choiceContext, creature, diff, creature, null, true);
        await PowerCmd.Apply<StrengthPower>(choiceContext, creature, diff, creature, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, creature, diff, creature, null);
    }

    private bool IsOwnCard(CardModel card) => card.Owner?.Creature == Owner;

    // An Ace entering or leaving Hand (played, discarded, exhausted, transformed...).
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (!IsOwnCard(card) || card is not SeelsAceOfSpade) return;
        await Sync(new ThrowingPlayerChoiceContext(), Owner);
    }

    // Cost changes (cost-reducing cards, turn-limited discounts expiring...) don't move the card,
    // so also resync after each card played and at the start of each turn.
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsOwnCard(cardPlay.Card)) return;
        await Sync(choiceContext, Owner);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await Sync(choiceContext, Owner);
    }

    public async Task AfterErraticCostRandomized(PlayerChoiceContext choiceContext, CardModel card, int cost)
    {
        if (!IsOwnCard(card) || card is not SeelsAceOfSpade) return;
        await Sync(choiceContext, Owner);
    }
}
