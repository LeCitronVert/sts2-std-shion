using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you apply Confusion, it targets ALL enemies and applies X additional Confusion.
/// Applications that already target all enemies (cards targeting ALL enemies, or effects wrapped in
/// <see cref="SlalMechanicsHelper.ApplyingToAllEnemies"/>) only get the additional Confusion.
/// </summary>
public class PinwheelForm : SlalTheEndlessPower
{
    private bool _isSpreading;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (_isSpreading || power is not Confusion || giver != Owner || target == null || target.Side == Owner.Side || amount <= 0)
            return 0m;
        return Amount;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_isSpreading || power is not Confusion || applier != Owner || amount <= 0 || CombatState is null) return;
        if (power.Owner.Side == Owner.Side) return;
        if (SlalMechanicsHelper.IsApplyingToAllEnemies || cardSource?.TargetType == TargetType.AllEnemies) return;

        var others = CombatState.HittableEnemies.Where(e => e != power.Owner).ToList();
        if (others.Count == 0) return;

        Flash();
        _isSpreading = true;
        try
        {
            foreach (var enemy in others)
            {
                await PowerCmd.Apply<Confusion>(choiceContext, enemy, amount, Owner, cardSource);
            }
        }
        finally
        {
            _isSpreading = false;
        }
    }
}
