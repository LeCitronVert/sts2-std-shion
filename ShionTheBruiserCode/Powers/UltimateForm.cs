using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// After X turns, lose ALL Bad Faith.
/// </summary>
public class UltimateForm : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BadFaith>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        Flash();
        await PowerCmd.Remove<BadFaith>(Owner);
        await PowerCmd.Remove(this);
    }
}
