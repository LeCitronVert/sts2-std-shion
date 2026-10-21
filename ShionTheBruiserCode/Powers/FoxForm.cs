using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the end of your turn, gain 2 Weak if you have none, otherwise gain X energy next turn and Y Strength.
/// Amount is the energy, the "Strength" var is stacked by the card.
/// </summary>
public class FoxForm : SlalTheEndlessPower
{
    public const int WeakAmount = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Strength", 0)];

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

        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, Amount, Owner, null);
        var strength = DynamicVars["Strength"].BaseValue;
        if (strength > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, strength, Owner, null);
        }
    }
}
