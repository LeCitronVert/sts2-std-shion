using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalFavouriteBucket(): SlalSlimyCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("PlatingAmount").WithMultiplier((card, _) => ((SlalFavouriteBucket)card).PreviewPlating())
    ];

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

        // Count Status and unplayable cards in player's hand (an unplayable Status counts once)
        var card = this;
        var amountOfBadCards = PileType
            .Hand.GetPile(card.Owner).Cards
            .Count(IsBadCard)
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

    private static bool IsBadCard(CardModel c) =>
        c.Type == CardType.Status || c.Keywords.Contains(CardKeyword.Unplayable);

    /// <summary>
    /// Plating this card would give if played now: bad cards in Hand (excluding itself)
    /// plus the Slimed it adds once it has left the Hand.
    /// </summary>
    private int PreviewPlating()
    {
        if (CombatState is null) return 0;

        var otherCards = PileType.Hand.GetPile(Owner).Cards.Where(c => c != this).ToList();
        return otherCards.Count(IsBadCard) + Math.Max(0, SlimedToAdd(otherCards.Count));
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}