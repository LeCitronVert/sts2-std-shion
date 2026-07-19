using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionAnOpening() : CritContributorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<CritRateUp>(5),
        new CardsVar(1),
        new CriticalDrawVar(15)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = play.Card;
        ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        await PowerCmd.Apply<CritRateUp>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars.Power<CritRateUp>().BaseValue,
            card.Owner.Creature,
            card
        );
        CardModel? card2 = (PileType
                .Draw
                .GetPile(card.Owner)
                .Cards
                .Where(c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(card.Owner.RunState.Rng.Shuffle).FirstOrDefault() ?? PileType.Draw.GetPile(card.Owner).Cards.Where(c => c.Type == CardType.Attack).ToList().StableShuffle(card.Owner.RunState.Rng.Shuffle).FirstOrDefault()) ?? null
        ;
        if (card2 == null)
            return;
        await CardPileCmd.Add(card2, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Power<CritRateUp>().UpgradeValueBy(5);
    }
    
    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
        
        return Task.CompletedTask;
    }
}