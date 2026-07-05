using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class CommanderCap() : CritContributorRelic, ICritAttackRateContributor, ICritHitRateContributor
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
        new DynamicVar("CritHitRate", 10),
        new DynamicVar("CritAttackRate", 5)
    ];

    public decimal GetCritAttackRateBonus(CardModel? sourceCard)
    {
        return sourceCard?.Owner.Creature == Owner.Creature ? DynamicVars["CritAttackRate"].BaseValue / 100 : 0;
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        return sourceCard?.Owner.Creature == Owner.Creature ? DynamicVars["CritAttackRate"].BaseValue / 100 : 0;
    }
}