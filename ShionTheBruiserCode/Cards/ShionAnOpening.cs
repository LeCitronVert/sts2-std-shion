using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionAnOpening() : CritContributorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<CritRateUp>(10),
        new CardsVar(1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = play.Card;
        await PowerCmd.Apply<AnOpening>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars.Power<CritRateUp>().BaseValue,
            card.Owner.Creature,
            card
        );
        CardModel card2 = PileType
            .Draw
            .GetPile(card.Owner)
            .Cards
            .Where<CardModel>((Func<CardModel, bool>) (c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable))).ToList<CardModel>().StableShuffle<CardModel>(card.Owner.RunState.Rng.Shuffle).FirstOrDefault<CardModel>() ?? PileType.Draw.GetPile(card.Owner).Cards.Where<CardModel>((Func<CardModel, bool>) (c => c.Type == CardType.Attack)).ToList<CardModel>().StableShuffle<CardModel>(card.Owner.RunState.Rng.Shuffle).FirstOrDefault<CardModel>()
        ;
        if (card2 == null)
            return;
        await CardPileCmd.Add(card2, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Power<CritRateUp>().UpgradeValueBy(5);
    }
}