using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public abstract class SlalSlimyCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    SlalTheEndlessCard(cost, type, rarity, target)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.Slimy];
    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != this || card.CombatState is null) return Task.CompletedTask;
        
        CardPileCmd.AddGeneratedCardToCombat(card.CombatState.CreateCard<Slimed>(card.Owner), PileType.Hand, card.Owner);

        return Task.CompletedTask;
    }
}