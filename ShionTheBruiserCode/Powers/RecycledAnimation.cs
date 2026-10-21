using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times you spend Stars in a turn, gain 1 energy.
/// </summary>
public class RecycledAnimation : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _triggersThisTurn;

    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        if (spender != Owner.Player || amount <= 0 || _triggersThisTurn >= Amount) return;

        _triggersThisTurn++;
        Flash();
        await PlayerCmd.GainEnergy(1, spender);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side) _triggersThisTurn = 0;
        return Task.CompletedTask;
    }
}
