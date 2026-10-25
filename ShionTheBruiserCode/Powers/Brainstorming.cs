using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Novel card, ALL other players gain X Random, however much Random you gained from it.
/// </summary>
public class Brainstorming : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Random>(),
    ];

    private class Data
    {
        /// <summary>Novel is removed when the card is played, so remember it before the play resolves.</summary>
        public readonly HashSet<CardModel> novelCardsBeingPlayed = [];
    }

    protected override object InitInternalData() => new Data();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && cardPlay.Card.Keywords.Contains(SlalCardKeywords.Novel))
        {
            GetInternalData<Data>().novelCardsBeingPlayed.Add(cardPlay.Card);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!GetInternalData<Data>().novelCardsBeingPlayed.Remove(cardPlay.Card) || CombatState is null) return;

        var others = CombatState.Allies.Where(c => c != Owner && c.IsAlive && c.IsPlayer).ToList();
        if (others.Count == 0) return;

        Flash();
        foreach (var other in others)
        {
            await SlalMechanicsHelper.GainRandom(choiceContext, other, Amount);
        }
    }
}
