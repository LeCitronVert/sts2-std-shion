using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
/// The first X times you play a Power each turn, reduce the cost of a random card in your Hand by 1 this turn.
/// Only cards that cost energy can be picked (not X-cost or Unplayable cards).
/// </summary>
public class BumperForm : SlalFirstPlaysEachTurnPower
{
    public const int CostReduction = 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    protected override bool ShouldTrigger(CardPlay cardPlay) => cardPlay.Card.Type == CardType.Power;

    protected override Task OnTriggered(PlayerChoiceContext choiceContext, CardPlay cardPlay, Player player)
    {
        var card = SlalMechanicsHelper.PickRandomCards(player, PileType.Hand, 1, c =>
            !c.EnergyCost.CostsX
            && !c.Keywords.Contains(CardKeyword.Unplayable)
            && SlalMechanicsHelper.GetEnergyCost(c) > 0
        ).FirstOrDefault();
        card?.EnergyCost.AddThisTurn(-CostReduction, true);
        return Task.CompletedTask;
    }
}
