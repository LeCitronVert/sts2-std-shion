using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you apply Vulnerable, gain (X times) as much Vigor next turn.
/// Goes through VigorNextTurn: Vulnerable can be applied mid-attack, which would break VigorPower.
/// </summary>
public class AggravatingDemeanor : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<VigorNextTurn>(),
    ];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not VulnerablePower || applier != Owner || power.Owner == Owner || amount <= 0) return;

        Flash();
        await PowerCmd.Apply<VigorNextTurn>(choiceContext, Owner, amount * Amount, Owner, null);
    }
}
