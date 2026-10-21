using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Applied by Anchor Throw on an enemy: whenever the applier plays a card that puts a card into their
/// Draw Pile, this enemy gains X Confusion.
/// Cards moved from the Discard Pile to the Draw Pile don't count, so shuffling doesn't trigger it.
/// </summary>
public class AnchoredDown : SlalTheEndlessPower
{
    public override string CustomPackedIconPath => "anchor_throw_2.png".PowerImagePath();
    public override string CustomBigIconPath => "anchor_throw_2.png".BigPowerImagePath();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    /// <summary>Cards currently being played by the applier, and whether they put a card into the Draw Pile.</summary>
    private readonly Dictionary<CardModel, bool> _playsInProgress = [];

    private bool IsApplierCard(CardModel card) => Applier?.Player != null && card.Owner == Applier.Player;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (IsApplierCard(cardPlay.Card))
        {
            _playsInProgress[cardPlay.Card] = false;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (_playsInProgress.Count == 0 || !IsApplierCard(card)) return Task.CompletedTask;
        if (card.Pile?.Type != PileType.Draw || oldPileType == PileType.Discard) return Task.CompletedTask;

        foreach (var playedCard in _playsInProgress.Keys.ToList())
        {
            _playsInProgress[playedCard] = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_playsInProgress.Remove(cardPlay.Card, out var putCardIntoDrawPile) || !putCardIntoDrawPile) return;
        if (!Owner.IsAlive) return;

        Flash();
        await PowerCmd.Apply<Confusion>(choiceContext, Owner, Amount, Applier, null);
    }
}
