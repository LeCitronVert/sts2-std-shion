using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Yellow challenge (on each player): the first card you play each turn costs 1 more (first N cards with N stacks).
/// </summary>
public class YellowEmeraldChallenge : EmeraldChallengePower
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;

    /// <summary>
    /// Display how many cards will still cost more this turn.
    /// </summary>
    public override int DisplayAmount => Math.Max(0, Amount - CardsPlayedThisTurn());

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this) InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player) InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldModifyCost(card, originalCost)) return false;
        modifiedCost = originalCost + 1;
        return true;
    }

    private bool ShouldModifyCost(CardModel card, decimal originalCost)
    {
        if (!CombatManager.Instance.IsInProgress) return false;
        if (card.Owner != Owner.Player || originalCost < 0 || card.EnergyCost.CostsX) return false;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return false;

        return CardsPlayedThisTurn() < Amount;
    }

    private int CardsPlayedThisTurn()
    {
        if (!CombatManager.Instance.IsInProgress) return 0;
        return CombatManager.Instance.History.CardPlaysStarted
            .Count(e => e.Actor == Owner && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(CombatState));
    }
}
