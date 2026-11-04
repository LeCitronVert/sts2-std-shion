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
/// Whenever you play a Wound, Exhaust it, gain X Vigor and draw 1 card.
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

    // The Wound is Exhausted instead of going to its usual pile.
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        return card.Owner.Creature == Owner && SeelsMechanicsHelper.IsWound(card) ? (PileType.Exhaust, position) : (pileType, position);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !SeelsMechanicsHelper.IsWound(cardPlay.Card) || Owner.Player is null) return;
        Flash();
        // AfterCardPlayed runs once the Wound has resolved, so this Vigor isn't added mid-attack.
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null);
        await CardPileCmd.Draw(choiceContext, Draw, Owner.Player);
    }
}
