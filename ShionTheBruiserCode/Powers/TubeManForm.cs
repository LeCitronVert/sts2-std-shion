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
/// Whenever you gain, lose or apply Weak, deal X damage to ALL enemies.
/// </summary>
public class TubeManForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not WeakPower || amount == 0 || CombatState is null) return;

        var gainedOrLost = power.Owner == Owner;
        var applied = applier == Owner && power.Owner != Owner && amount > 0;
        if (!gainedOrLost && !applied) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies.ToList(), Amount, ValueProp.Unpowered, Owner, null);
    }
}
