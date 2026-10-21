using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you draw a card during your turn, gain (X times) its energy cost as Block.
/// </summary>
public class GamblersLuck : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner || Owner.CombatState?.CurrentSide != CombatSide.Player) return;

        var block = card.EnergyCost.GetResolved() * Amount;
        if (block <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(Owner, block, ValueProp.Unpowered, null);
    }
}
