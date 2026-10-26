using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class ScopeLens :  CritContributorRelic, ICritHitRateContributor
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
        new DynamicVar("CritAttackRate", 10)
    ];

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard == null || sourceCard.Owner.Creature == Owner.Creature)
        {
            decimal bonus = DynamicVars["CritAttackRate"].BaseValue / 100;
            return bonus;
        }
        return 0;
    }
}