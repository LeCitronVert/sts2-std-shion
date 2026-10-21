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
/// Whenever you gain Vulnerable, gain Block next turn equal to your Vulnerable, up to 5 (10). Gain 2 (4) Vulnerable.
/// </summary>
public class SeelsAggravatingDemeanor() : SeelsTheRecklessCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<AggravatingDemeanor>(5M),
        new PowerVar<VulnerablePower>(2M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<AggravatingDemeanor>(choiceContext, creature, DynamicVars["AggravatingDemeanor"].BaseValue, creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, creature, DynamicVars.Vulnerable.BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AggravatingDemeanor"].UpgradeValueBy(5M);
        DynamicVars.Vulnerable.UpgradeValueBy(2M);
    }
}
