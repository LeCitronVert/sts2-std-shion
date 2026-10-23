using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you reach 10 Random, and whenever you play a card from your Hand that creates a card,
/// choose 1 of 3 cards from any character to add into your Hand (X times). Can be skipped.
/// </summary>
public class GenieForm : SlalTheEndlessPower, ISlalRandomListener
{
    public const int CardsToChooseFrom = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Random>(),
    ];

    private class Data
    {
        /// <summary>Cards played from the Hand that are still resolving, and whether they created a card.</summary>
        public readonly Dictionary<CardModel, bool> playsInProgress = [];

        /// <summary>True while Genie Form is adding its own card, which doesn't count as "a card that creates a card".</summary>
        public bool offering;
    }

    protected override object InitInternalData() => new Data();

    public async Task AfterRandomThresholdReached(PlayerChoiceContext choiceContext, Creature gainer)
    {
        if (gainer != Owner) return;
        await OfferCards(choiceContext);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && !cardPlay.IsAutoPlay && cardPlay.IsFirstInSeries)
        {
            GetInternalData<Data>().playsInProgress[cardPlay.Card] = false;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        var data = GetInternalData<Data>();
        var plays = data.playsInProgress;
        if (creator != Owner.Player || plays.Count == 0 || data.offering) return Task.CompletedTask;

        foreach (var playedCard in plays.Keys.ToList())
        {
            plays[playedCard] = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.IsLastInSeries) return;
        if (!GetInternalData<Data>().playsInProgress.Remove(cardPlay.Card, out var createdACard) || !createdACard) return;

        await OfferCards(choiceContext);
    }

    private async Task OfferCards(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is null || CombatState is null) return;

        Flash();
        var data = GetInternalData<Data>();
        var options = SlalMechanicsHelper.GetCharacterCardOptions(Owner.Player, includeOwnCharacter: true);
        data.offering = true;
        try
        {
            for (var i = 0; i < Amount; i++)
            {
                await SlalMechanicsHelper.ChooseCardToAddToHand(choiceContext, Owner.Player, options, CardsToChooseFrom, false);
            }
        }
        finally
        {
            data.offering = false;
        }
    }
}
