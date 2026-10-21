using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalFavouriteBucket(): SlalSlimyCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        SlalCardKeywords.Slimy,
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PlatingPower>(),
    ];
    
    protected override bool SlimesBeforeEffect => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // Slimy applies first, so the Slimed added count as Status cards
        await AddSlimed();

        // Count amount of status and curses in player's hand
        var card = this;
        var amountOfBadCards = PileType
            .Hand.GetPile(card.Owner).Cards
            .Where(c => c.Type is CardType.Status or CardType.Curse)
            .ToList().Count
        ;

        if (amountOfBadCards <= 0) return;
        
        await PowerCmd.Apply<PlatingPower>(
            choiceContext, 
            card.Owner.Creature, 
            amountOfBadCards, 
            card.Owner.Creature, 
            card
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}