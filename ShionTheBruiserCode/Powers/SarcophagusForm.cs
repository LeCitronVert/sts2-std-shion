using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// You take X less damage from Weak enemies (per hit). Whenever you gain Weak, gain Y energy next turn.
/// Amount is the damage reduction, the "Energy" var is stacked by the card.
/// </summary>
public class SarcophagusForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(0)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.ForEnergy(this),
    ];

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer.Side == Owner.Side || !props.IsPoweredAttack() || !dealer.HasPower<WeakPower>()) return 0m;
        return -Math.Min(Amount, Math.Max(0m, amount));
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not WeakPower || amount <= 0 || power.Owner != Owner) return;

        var energy = DynamicVars["Energy"].BaseValue;
        if (energy <= 0) return;
        Flash();
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, energy, Owner, null);
    }
}
