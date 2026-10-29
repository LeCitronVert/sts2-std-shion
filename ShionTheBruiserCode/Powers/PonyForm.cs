using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Novel cards grant X additional Random (handled in <see cref="SlalMechanicsHelper.GetNovelRandom"/>).
/// The first time you play a Novel card each turn, add a Fresh Idea into your Hand.
/// </summary>
public class PonyForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Random>(),
        HoverTipFactory.FromCard<SlalFreshIdea>(),
    ];

    private class Data
    {
        /// <summary>Novel is removed when the card is played, so remember it before the play resolves.</summary>
        public readonly HashSet<CardModel> novelCardsBeingPlayed = [];
        public bool triggeredThisTurn;
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner)) GetInternalData<Data>().triggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && SlalMechanicsHelper.IsNovel(cardPlay.Card))
        {
            GetInternalData<Data>().novelCardsBeingPlayed.Add(cardPlay.Card);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        if (!data.novelCardsBeingPlayed.Remove(cardPlay.Card) || data.triggeredThisTurn || Owner.Player is null || CombatState is null) return;

        data.triggeredThisTurn = true;
        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<SlalFreshIdea>(Owner.Player), PileType.Hand, Owner.Player);
    }
}
