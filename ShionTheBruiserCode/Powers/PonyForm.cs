using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the end of your turn, put ALL non-Ethereal cards in your Hand into your Draw Pile.
/// They gain Ethereal and cost X less until played.
/// </summary>
public class PonyForm : SlalTheEndlessPower
{
    private class Data
    {
        public readonly HashSet<CardModel> etherealizedCards = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

    protected override object InitInternalData() => new Data();

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        var cards = PileType.Hand.GetPile(player).Cards
            .Where(c => !c.Keywords.Contains(CardKeyword.Ethereal))
            .ToList();
        if (cards.Count == 0) return;

        Flash();
        var data = GetInternalData<Data>();
        foreach (var card in cards)
        {
            card.AddKeyword(CardKeyword.Ethereal);
            data.etherealizedCards.Add(card);
            if (!card.EnergyCost.CostsX)
            {
                card.EnergyCost.AddUntilPlayed(-Amount, true);
            }
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random);
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Ethereal only lasts until the card is played.
        if (GetInternalData<Data>().etherealizedCards.Remove(cardPlay.Card))
        {
            cardPlay.Card.RemoveKeyword(CardKeyword.Ethereal);
        }
        return Task.CompletedTask;
    }
}
