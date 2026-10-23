using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Choose up to X cards in your Hand and Transform them into random Goofball Token Powers. They are free to play this turn.
/// </summary>
public class SlalEmergencyShapeshift() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var amount = card.ResolveEnergyXValue();
        if (amount <= 0) return;

        var targets = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, amount),
            c => c.IsTransformable,
            card
        )).ToList();

        foreach (var target in targets)
        {
            var power = SlalMechanicsHelper.CreateGoofballTokenPower(card.Owner, IsUpgraded);
            if (!power.EnergyCost.CostsX)
            {
                power.SetToFreeThisTurn();
            }
            await CardCmd.Transform(target, power);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
