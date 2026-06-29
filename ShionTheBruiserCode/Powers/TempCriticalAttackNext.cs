using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class TempCriticalAttackNext : CustomTemporaryPowerModelWrapper<ShionTouchOfForesight, CriticalAttackNext>
{
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (Owner == power.Owner && power is CriticalAttackNext && amount < 0)
        {
            await PowerCmd.Remove(this);
        }
        
        await base.AfterPowerAmountChanged(context, power, amount, applier, cardSource);
    }
}