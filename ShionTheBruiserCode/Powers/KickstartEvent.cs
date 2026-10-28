using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// This turn, whenever you draw a Novel card, play it and draw X cards.
/// </summary>
public class KickstartEvent : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
    ];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != Owner.Player || Owner.Player is null) return;
        if (!card.Keywords.Contains(SlalCardKeywords.Novel) || card.Keywords.Contains(CardKeyword.Unplayable)) return;
        if (card.Pile?.Type != PileType.Hand || CombatManager.Instance.IsOverOrEnding) return;

        Flash();
        var amount = Amount;
        await CardCmd.AutoPlay(choiceContext, card, null);
        await CardPileCmd.Draw(choiceContext, amount, Owner.Player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
