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
/// Whenever you apply Weak, gain X Block. Whenever you gain Weak, gain Y energy next turn.
/// Amount is the Block, the "Energy" var is stacked by the card.
/// </summary>
public class SarcophagusForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Energy", 0)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.ForEnergy(this),
    ];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not WeakPower || amount <= 0) return;

        if (power.Owner == Owner)
        {
            var energy = DynamicVars["Energy"].BaseValue;
            if (energy <= 0) return;
            Flash();
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, energy, Owner, null);
        }
        else if (applier == Owner)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
    }
}
