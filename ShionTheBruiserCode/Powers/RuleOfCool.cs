using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you attack an enemy and it has less HP than (X times) the amount of energy and Stars you spent this turn, it dies.
/// </summary>
public class RuleOfCool : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => _spentThisTurn * Amount;

    private int _spentThisTurn;

    public override Task AfterEnergySpent(CardModel card, int amount)
    {
        if (card.Owner.Creature == Owner) AddSpent(amount);
        return Task.CompletedTask;
    }

    public override Task AfterStarsSpent(int amount, Player spender)
    {
        if (spender == Owner.Player) AddSpent(amount);
        return Task.CompletedTask;
    }

    private void AddSpent(int amount)
    {
        _spentThisTurn += amount;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner) return;

        var threshold = _spentThisTurn * Amount;
        var victims = command.Results.SelectMany(r => r)
            .Select(r => r.Receiver)
            .Distinct()
            .Where(c => c.IsAlive && c.Side != Owner.Side && c.CurrentHp < threshold)
            .ToList();

        foreach (var victim in victims)
        {
            Flash();
            await CreatureCmd.Kill(victim);
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            _spentThisTurn = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
