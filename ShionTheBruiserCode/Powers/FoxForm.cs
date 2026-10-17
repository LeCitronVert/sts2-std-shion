using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the end of your turn, gain 2 Weak if you have none, otherwise gain X Strength and X energy next turn.
/// </summary>
public class FoxForm : SlalTheEndlessPower
{
    public const int WeakAmount = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.ForEnergy(this),
    ];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;

        Flash();
        if (!Owner.HasPower<WeakPower>())
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, Owner, WeakAmount, Owner, null);
            return;
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
