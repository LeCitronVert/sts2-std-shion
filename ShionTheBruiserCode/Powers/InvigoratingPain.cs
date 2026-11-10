using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play or Exhaust a Wound from your Hand, gain X Vigor and draw 1 card.
/// A played Wound that then gets Exhausted (from the Play pile) only triggers once.
/// </summary>
public class InvigoratingPain : SeelsTheRecklessPower
{
    public const int Draw = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Wound>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    // Wounds that just went from the Hand to the Exhaust Pile (AfterCardExhausted doesn't say where it came from).
    private class Data
    {
        public readonly HashSet<CardModel> exhaustedFromHand = [];
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (oldPileType == PileType.Hand && card.Pile?.Type == PileType.Exhaust && IsOwnWound(card))
        {
            GetInternalData<Data>().exhaustedFromHand.Add(card);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (!GetInternalData<Data>().exhaustedFromHand.Remove(card)) return;
        await Trigger(choiceContext);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsOwnWound(cardPlay.Card)) return;
        // AfterCardPlayed runs once the Wound has resolved, so this Vigor isn't added mid-attack.
        await Trigger(choiceContext);
    }

    private bool IsOwnWound(CardModel card) => card.Owner.Creature == Owner && SeelsMechanicsHelper.IsWound(card);

    private async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is null) return;
        Flash();
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null);
        await CardPileCmd.Draw(choiceContext, Draw, Owner.Player);
    }
}
