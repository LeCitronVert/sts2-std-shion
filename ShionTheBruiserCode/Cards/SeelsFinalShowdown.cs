using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 3 (5) Vulnerable. Gain Strength and Demise equal to the enemy's Vulnerable. Draw 3 (5) cards.
/// </summary>
//todo : choose cost
public class SeelsFinalShowdown() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VulnerablePower>(3M),
        new CardsVar(3),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ShowdownAmount").WithMultiplier((card, target) =>
            target is null ? 0 : ((SeelsFinalShowdown)card).VulnerableAfterApply(target)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DemisePower>(),
    ];

    // Preview only: an enemy that already has Artifact blocks the Vulnerable.
    private decimal VulnerableAfterApply(Creature target)
    {
        var applied = target.GetPowerAmount<ArtifactPower>() > 0 ? 0 : DynamicVars.Vulnerable.BaseValue;
        return SeelsMechanicsHelper.GetVulnerable(target) + applied;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);

        var amount = SeelsMechanicsHelper.GetVulnerable(play.Target);
        if (amount > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
            await PowerCmd.Apply<DemisePower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(2M);
        DynamicVars.Cards.UpgradeValueBy(2M);
    }
}
