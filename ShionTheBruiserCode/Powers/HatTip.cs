using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Slimy card, add X Slimed to your Discard Pile.
/// </summary>
public class HatTip : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
        HoverTipFactory.FromCard<Slimed>(),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !SlalMechanicsHelper.IsSlimy(cardPlay.Card)) return;

        Flash();
        var slimed = Enumerable.Range(0, Amount).Select(_ => SlalMechanicsHelper.CreateCard<Slimed>(Owner.Player)).ToList();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(slimed, PileType.Discard, Owner.Player));
    }
}
