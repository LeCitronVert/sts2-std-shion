using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Each turn, the first X HP you should lose are not lost. You gain Damage Debt instead.
/// (Like Hardened Shell, but the other way around.)
/// </summary>
public class BadFaith : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => Math.Max(0, Amount - _absorbedThisTurn);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DamageDebt>()];

    private int _absorbedThisTurn;
    private int _pendingDebt;

    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0 || FryMechanicsHelper.IsPayingDebt) return amount;

        var absorbed = (int)Math.Min(amount, DisplayAmount);
        if (absorbed <= 0) return amount;

        _absorbedThisTurn += absorbed;
        _pendingDebt += absorbed;
        return amount - absorbed;
    }

    public override async Task AfterModifyingHpLostBeforeOsty()
    {
        if (_pendingDebt <= 0) return;

        var debt = _pendingDebt;
        _pendingDebt = 0;
        Flash();
        InvokeDisplayAmountChanged();
        await MegaCrit.Sts2.Core.Commands.PowerCmd.Apply<DamageDebt>(new ThrowingPlayerChoiceContext(), Owner, debt, Owner, null);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            _absorbedThisTurn = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
