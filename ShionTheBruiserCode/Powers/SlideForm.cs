using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X Novel cards you play each turn draw 1 card.
/// </summary>
public class SlideForm : SlalFirstPlaysEachTurnPower
{
    public const int CardsDrawn = 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
    ];

    protected override bool ShouldTrigger(CardPlay cardPlay) => SlalMechanicsHelper.IsNovel(cardPlay.Card);

    protected override async Task OnTriggered(PlayerChoiceContext choiceContext, CardPlay cardPlay, Player player)
    {
        await CardPileCmd.Draw(choiceContext, CardsDrawn, player);
    }
}
