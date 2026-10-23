using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Slimy: after playing this card, add a Slimed into your Hand.
/// Very Slimy: after playing this card, fill your Hand with Slimed.
/// </summary>
public abstract class SlalSlimyCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    SlalTheEndlessCard(cost, type, rarity, target)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.Slimy];

    /// <summary>
    /// When true, the Slimed are added at the start of the card's effect instead of after playing it.
    /// The card's OnPlay must then call <see cref="AddSlimed"/> itself.
    /// </summary>
    protected virtual bool SlimesBeforeEffect => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this || SlimesBeforeEffect) return;
        await AddSlimed();
    }

    /// <summary>
    /// Number of Slimed this card adds, given how many cards are in the Hand at that moment.
    /// </summary>
    protected int SlimedToAdd(int cardsInHand) =>
        Keywords.Contains(SlalCardKeywords.VerySlimy)
            ? CardPile.MaxCardsInHand - cardsInHand
            : Keywords.Contains(SlalCardKeywords.Slimy) ? 1 : 0;

    protected async Task AddSlimed()
    {
        if (CombatState is null) return;

        var amount = SlimedToAdd(PileType.Hand.GetPile(Owner).Cards.Count);
        if (amount <= 0) return;

        List<CardModel> slimed = [];
        for (var i = 0; i < amount; i++)
        {
            slimed.Add(CombatState.CreateCard<Slimed>(Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(slimed, PileType.Hand, Owner);
    }
}
