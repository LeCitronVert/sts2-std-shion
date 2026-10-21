using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Take an extra turn after this one.
/// </summary>
public class FryExtraTurn : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldTakeExtraTurn(Player player)
    {
        return player == Owner.Player;
    }

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player == Owner.Player)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
