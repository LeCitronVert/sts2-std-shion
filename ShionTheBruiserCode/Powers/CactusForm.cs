using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the end of your turn, lose up to X energy. Gain 1 Thorns for each energy lost this way.
/// </summary>
public class CactusForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ThornsPower>(),
    ];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is null) return;

        var energyLost = Math.Min(Amount, Owner.Player.PlayerCombatState?.Energy ?? 0);
        if (energyLost <= 0) return;

        Flash();
        await PlayerCmd.LoseEnergy(energyLost, Owner.Player);
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, energyLost, Owner, null);
    }
}
