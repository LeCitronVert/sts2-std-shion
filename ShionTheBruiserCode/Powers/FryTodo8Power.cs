using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// If you play 4 or less cards in a turn, gain X energy next turn.
/// </summary>
public class FryTodo8Power : FryTheCommanderPower
{
    public const int MaxCards = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _cardsPlayedThisTurn;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner) _cardsPlayedThisTurn++;
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;

        var played = _cardsPlayedThisTurn;
        _cardsPlayedThisTurn = 0;
        if (played > MaxCards) return;

        Flash();
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
