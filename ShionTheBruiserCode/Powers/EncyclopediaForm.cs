using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Walking Encyclopedia Form: whenever you play a non-Novel card from your Hand, gain X Random.
/// Novel is removed when the card is played, so it is checked before the play resolves
/// (which also keeps the Form card itself from triggering it).
/// </summary>
public class EncyclopediaForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Random>(),
    ];

    private class Data
    {
        public readonly HashSet<CardModel> cardsBeingPlayed = [];
    }

    protected override object InitInternalData() => new Data();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && !cardPlay.IsAutoPlay && cardPlay.IsFirstInSeries
            && !cardPlay.Card.Keywords.Contains(SlalCardKeywords.Novel))
        {
            GetInternalData<Data>().cardsBeingPlayed.Add(cardPlay.Card);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.IsLastInSeries || !GetInternalData<Data>().cardsBeingPlayed.Remove(cardPlay.Card)) return;

        Flash();
        await SlalMechanicsHelper.GainRandom(choiceContext, Owner, Amount);
    }
}
