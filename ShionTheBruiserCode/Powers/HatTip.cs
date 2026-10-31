using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a card with Shapeshift or Mimic, Free Shapeshift X times.
/// The cards are recognized by their Shapeshift / Mimic tags (the Goofball Token Powers it adds have neither).
/// </summary>
public class HatTip : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
        HoverTipFactory.FromKeyword(SlalCardKeywords.FreeShapeshift),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner.Player || Owner.Player is null) return;
        if (!card.Tags.Contains(SlalCardTags.Shapeshift) && !card.Tags.Contains(SlalCardTags.Mimic)) return;

        Flash();
        await SlalMechanicsHelper.Shapeshift(Owner.Player, Amount, free: true);
    }
}
