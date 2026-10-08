using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class GorillaForm(): SlalTheEndlessPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner))
            return Task.CompletedTask;

        var vigorAmount = Amount * Owner.GetPowerAmount<Random>();
        if (0 == vigorAmount) return Task.CompletedTask;
        
        Flash();
        PowerCmd.Apply<VigorPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            vigorAmount,
            Owner,
            null!
        );
        
        return Task.CompletedTask;
    }
}