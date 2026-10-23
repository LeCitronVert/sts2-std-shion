using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add a random (upgraded) Colorless card into your Draw Pile. Gain Vigor equal to its energy cost (0 for X-cost cards).
/// </summary>
public class SlalPlotTwist() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var colorless = ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint);
        foreach (var newCard in SlalMechanicsHelper.CreateRandomCards(card.Owner, 1, IsUpgraded, colorless))
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Draw, card.Owner, CardPilePosition.Random));

            var vigor = SlalMechanicsHelper.GetEnergyCost(newCard);
            if (vigor > 0)
            {
                await PowerCmd.Apply<VigorPower>(choiceContext, card.Owner.Creature, vigor, card.Owner.Creature, card);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
