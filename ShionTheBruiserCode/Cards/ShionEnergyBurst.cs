using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Turtle Hermit Shell starter. Gain 3 * X Vigor. Gain X Vulnerable.
/// </summary>
public class ShionEnergyBurst() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("VigorPerX", 3),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("VigorAmount").WithMultiplier((card, _) => ((ShionEnergyBurst)card).PreviewX() * card.DynamicVars["VigorPerX"].IntValue),
        new CalculatedVar("VulnerableAmount").WithMultiplier((card, _) => ((ShionEnergyBurst)card).PreviewX()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    private int PreviewX() => CombatState is null ? 0 : PreviewHelper.XValue(this);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;
        var x = card.ResolveEnergyXValue();
        if (x == 0) return;

        await CreatureCmd.TriggerAnim(
            owner,
            Character.ShionTheBruiser.GetEnergyCastAnimIfApplicable(card.Owner.Character),
            Character.ShionTheBruiser.GetEnergyCastDelayIfApplicable(card.Owner.Character)
        );
        await PowerCmd.Apply<VigorPower>(choiceContext, owner, x * card.DynamicVars["VigorPerX"].BaseValue, owner, card);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, owner, x, owner, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPerX"].UpgradeValueBy(1);
    }
}
