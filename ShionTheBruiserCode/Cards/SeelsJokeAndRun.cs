using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 3 Vulnerable. Gain 1 (2) Dexterity. Heal HP equal to your Vulnerable.
/// </summary>
public class SeelsJokeAndRun() : SeelsTheRecklessCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VulnerablePower>(3M),
        new PowerVar<DexterityPower>(1M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Counts the Vulnerable gained first
        new CalculatedVar("HealAmount").WithMultiplier((card, _) =>
            SeelsMechanicsHelper.GetVulnerable(card.Owner.Creature) + card.DynamicVars.Vulnerable.BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<VulnerablePower>(choiceContext, creature, DynamicVars.Vulnerable.BaseValue, creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, creature, DynamicVars.Dexterity.BaseValue, creature, this);

        var heal = SeelsMechanicsHelper.GetVulnerable(creature);
        if (heal > 0)
        {
            await CreatureCmd.Heal(creature, heal);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(1M);
    }
}
