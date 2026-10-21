using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// This turn, double the Confusion you apply to enemies with less Confusion than you have cards in your Discard Pile.
/// </summary>
public class BurstOfLaughter : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is not Confusion || giver != Owner || target == null || target == Owner || Owner.Player == null || amount <= 0)
            return 1m;

        var discardCount = PileType.Discard.GetPile(Owner.Player).Cards.Count;
        return target.GetPowerAmount<Confusion>() < discardCount ? 2m : 1m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
