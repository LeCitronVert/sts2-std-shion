using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever a card with Retain is put into your Discard Pile, draw X cards and gain 4 Block (per stack).
/// </summary>
public class FryTodo11Power : FryTheCommanderPower
{
    public const int BlockPerStack = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(BlockPerStack, ValueProp.Unpowered)];

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner.Creature != Owner || card.Pile?.Type != PileType.Discard || oldPileType == PileType.Discard) return;
        if (!card.Keywords.Contains(CardKeyword.Retain)) return;

        Flash();
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player!);
        await CreatureCmd.GainBlock(Owner, BlockPerStack * Amount, ValueProp.Unpowered, null);
    }
}
