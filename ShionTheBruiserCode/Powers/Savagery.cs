using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times you play an Attack each turn, a random card in your Hand is free to play that turn.
/// The counter shows how many triggers are left this turn.
/// </summary>
public class Savagery : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => Math.Max(0, Amount - _usedThisTurn);

    private int _usedThisTurn;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Player;
        if (player is null || cardPlay.Card.Owner != player || cardPlay.Card.Type != CardType.Attack || !cardPlay.IsFirstInSeries) return Task.CompletedTask;
        if (DisplayAmount <= 0) return Task.CompletedTask;

        _usedThisTurn++;
        InvokeDisplayAmountChanged();
        Flash();

        // Prefer cards that actually cost something; a card that's already free gains nothing.
        var hand = PileType.Hand.GetPile(player).Cards.Where(c => c != cardPlay.Card).ToList();
        var costly = hand.Where(c => !c.EnergyCost.CostsX && c.EnergyCost.GetWithModifiers(CostModifiers.All) > 0).ToList();
        var pool = costly.Count > 0 ? costly : hand;
        if (pool.Count == 0) return Task.CompletedTask;

        var card = player.RunState.Rng.CombatCardSelection.NextItem(pool);
        card?.SetToFreeThisTurn();
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            _usedThisTurn = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
