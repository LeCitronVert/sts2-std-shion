using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever one of your Requested counters reaches 0, gain X Command.
/// </summary>
public class SummonBiggerFish : FryTheCommanderPower, IFryListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Request),
        HoverTipFactory.FromPower<Command>(),
    ];

    public async Task AfterRequestCounterReachedZero(PlayerChoiceContext choiceContext, Requested request)
    {
        if (request.Owner != Owner || Owner.IsDead) return;
        Flash();
        await FryMechanicsHelper.GainCommand(choiceContext, Owner, Amount, null);
    }
}
