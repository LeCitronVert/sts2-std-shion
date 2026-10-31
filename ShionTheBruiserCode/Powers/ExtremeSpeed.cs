using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

// Amount = turns of Slow remaining. Each play of Extreme Speed adds one turn.
public class ExtremeSpeed() : CustomTemporaryPowerModelWrapper<ShionExtremeSpeed, SlowPower>
{
    public override PowerType Type => PowerType.Debuff;
    
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // Same timing as the base temporary power: tick at the end of the opposing side's turn.
        if (Amount > 1 && !participants.Contains(Owner))
        {
            // Base AfterPowerAmountChanged mirrors the -1 onto Slow, keeping both in sync.
            await PowerCmd.Decrement(this);
            return;
        }

        await base.AfterSideTurnEnd(choiceContext, side, participants);
    }
}
