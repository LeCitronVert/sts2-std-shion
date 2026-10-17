using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalChaoticClone() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6M, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);

        var slimedCards = PileType.Hand.GetPile(card.Owner).Cards.Where(c => c is Slimed).ToList();
        List<CardModel> toPlay = IsUpgraded
            ? slimedCards
            : slimedCards.TakeRandom(1, card.Owner.RunState.Rng.CombatCardSelection).ToList();

        foreach (var slimed in toPlay)
        {
            await CardCmd.AutoPlay(choiceContext, slimed, null);
        }

        // Retain is added after the cards are played.
        if (toPlay.Count == 0) return;
        var chosenCards = await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(card.SelectionScreenPrompt, toPlay.Count),
            c => !c.Keywords.Contains(CardKeyword.Retain),
            card
        );
        foreach (var chosenCard in chosenCards)
        {
            chosenCard.AddKeyword(CardKeyword.Retain);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
