using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever one of your Requested counters reaches 0, gain X energy.
/// </summary>
public class FryTodo17Power : FryTheCommanderPower, IFryListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Requested>()];

    public async Task AfterRequestCounterReachedZero(PlayerChoiceContext choiceContext, Requested request)
    {
        if (request.Owner != Owner) return;

        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player!);
    }
}
