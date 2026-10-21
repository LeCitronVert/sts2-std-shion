using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you hit an enemy, if it doesn't lose more HP than your Weak, gain X energy.
/// </summary>
public class PirateParrotForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.ForEnergy(this),
    ];

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || Owner.Player == null || target.Side == Owner.Side || !props.IsPoweredAttack()) return;
        if (result.UnblockedDamage > Owner.GetPowerAmount<WeakPower>()) return;

        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
    }
}
