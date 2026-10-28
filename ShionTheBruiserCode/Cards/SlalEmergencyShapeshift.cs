using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Choose up to 3 cards in your Hand and Transform them into random Goofball Token Powers (all different).
/// They are free to play this turn. Exhaust.
/// </summary>
public class SlalEmergencyShapeshift() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var targets = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, card.DynamicVars.Cards.IntValue),
            c => c.IsTransformable,
            card
        )).ToList();
        if (targets.Count == 0) return;

        var powers = SlalMechanicsHelper.CreateDifferentGoofballTokenPowers(card.Owner, targets.Count, IsUpgraded);
        for (var i = 0; i < targets.Count; i++)
        {
            var power = powers[i];
            if (!power.EnergyCost.CostsX)
            {
                power.SetToFreeThisTurn();
            }
            await CardCmd.Transform(targets[i], power);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
