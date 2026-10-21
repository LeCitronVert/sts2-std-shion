using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// If you lose HP at least once during your turn, draw X additional cards at the start of your next turn.
/// </summary>
public class BleedInstructions : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _lostHpThisTurn;
    private bool _drawBonusNextTurn;

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0 && Owner.CombatState?.CurrentSide == Owner.Side)
        {
            _lostHpThisTurn = true;
        }
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            _drawBonusNextTurn = _lostHpThisTurn;
            _lostHpThisTurn = false;
        }
        return Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player == Owner.Player && _drawBonusNextTurn ? count + Amount : count;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) _drawBonusNextTurn = false;
        return Task.CompletedTask;
    }
}
