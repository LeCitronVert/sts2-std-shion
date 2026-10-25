using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Discard your Hand. Another player gains X energy and draws as many cards as you discarded this way.
/// Upgraded: they draw cards until their Hand is full instead.
/// </summary>
public class SlalBicycleTime() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Cards discarded: the rest of your Hand
        new CalculatedVar("DrawAmount").WithMultiplier((card, _) =>
            PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card)),
        new CalculatedVar("EnergyAmount").WithMultiplier((card, _) => PreviewHelper.XValue(card)),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target?.Player);
        var receiver = play.Target.Player;
        var x = card.ResolveEnergyXValue();

        var hand = PileType.Hand.GetPile(card.Owner).Cards.ToList();
        if (hand.Count > 0)
        {
            await CardCmd.Discard(choiceContext, hand);
        }

        if (x > 0)
        {
            await PlayerCmd.GainEnergy(x, receiver);
        }

        var toDraw = IsUpgraded
            ? CardPile.MaxCardsInHand - PileType.Hand.GetPile(receiver).Cards.Count
            : hand.Count;
        if (toDraw > 0)
        {
            await CardPileCmd.Draw(choiceContext, toDraw, receiver);
        }
    }
}
