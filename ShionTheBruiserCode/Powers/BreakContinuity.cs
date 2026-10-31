using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Novel card, Free Shapeshift X times, gain 1 Frail and 1 Disintegration.
/// Novel is removed when the card is played, so it is checked before the play resolves.
/// </summary>
public class BreakContinuity : SlalTheEndlessPower
{
    public const int FrailAmount = 1;
    public const int DisintegrationAmount = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromKeyword(SlalCardKeywords.FreeShapeshift),
        HoverTipFactory.FromPower<FrailPower>(),
        HoverTipFactory.FromPower<DisintegrationPower>(),
    ];

    private class Data
    {
        public readonly HashSet<CardModel> novelPlays = [];
    }

    protected override object InitInternalData() => new Data();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && cardPlay.IsFirstInSeries && SlalMechanicsHelper.IsNovel(cardPlay.Card))
        {
            GetInternalData<Data>().novelPlays.Add(cardPlay.Card);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.IsLastInSeries || !GetInternalData<Data>().novelPlays.Remove(cardPlay.Card) || Owner.Player is null) return;

        Flash();
        await SlalMechanicsHelper.Shapeshift(Owner.Player, Amount, free: true);
        await PowerCmd.Apply<FrailPower>(choiceContext, Owner, FrailAmount, Owner, null);
        await PowerCmd.Apply<DisintegrationPower>(choiceContext, Owner, DisintegrationAmount, Owner, null);
    }
}
