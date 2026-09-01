using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalQuickMeal(): SlalSlimyCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        List<CardModel> list = PileType.Hand.GetPile(card.Owner).Cards.ToList();
        int cardCount = list.Count;
        foreach (CardModel card2 in list)
            await CardCmd.Exhaust(choiceContext, card2);
        if (cardCount <= 0) return;
        await CreatureCmd.Heal(
            card.Owner.Creature,
            cardCount * card.DynamicVars.Heal.BaseValue
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}