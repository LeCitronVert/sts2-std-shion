using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you apply Weak to an enemy, apply 1 Vulnerable to that enemy. Doesn't stack.
/// </summary>
public class AnglerfishForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public const int VulnerableAmount = 1;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not WeakPower || amount <= 0 || applier != Owner || power.Owner.Side == Owner.Side || !power.Owner.IsAlive) return;

        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, power.Owner, VulnerableAmount, Owner, cardSource);
    }
}
