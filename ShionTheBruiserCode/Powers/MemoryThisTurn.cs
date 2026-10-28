using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Memory that only lasts this turn: triggers at the end of your turn, then is removed.
/// If the owner also has Memory, Memory plays both amounts at once so no card is played twice.
/// </summary>
public class MemoryThisTurn : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
    ];

    // Memory adds the bonus itself when the owner has both
    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is not Confusion || amount <= 0 || cardSource?.Owner?.Creature != Owner || Owner.HasPower<Memory>()) return 0m;
        return SlalMechanicsHelper.GetMemoryConfusionBonus(cardSource);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is null) return;

        Flash();
        if (Owner.HasPower<Memory>()) return;
        await SlalMechanicsHelper.PlayFromMemory(choiceContext, Owner.Player, Amount);
    }

    // Removed after the turn ends rather than right after playing, so Memory can still count it whichever triggers first
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
