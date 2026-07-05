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
        MainFile.Logger.Info($"[CommanderCap] GetCritAttackRateBonus called. sourceCard={sourceCard?.GetType().Name ?? "null"}, Owner.Creature={Owner.Creature?.GetType().Name ?? "null"}");
        
        // Relics contribute to tooltip (null) and to attacks by their owner
        // null check prevents cross-character bleeding
        if (sourceCard == null || sourceCard.Owner.Creature == Owner.Creature)
        {
            decimal bonus = DynamicVars["CritAttackRate"].BaseValue / 100;
            MainFile.Logger.Info($"[CommanderCap] Returning CritAttackRate bonus={bonus:P0}");
            return bonus;
        }
        MainFile.Logger.Info($"[CommanderCap] sourceCard owner mismatch, returning 0");
        return 0;
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        MainFile.Logger.Info($"[CommanderCap] GetCritHitRateBonus called. sourceCard={sourceCard?.GetType().Name ?? "null"}, Owner.Creature={Owner.Creature?.GetType().Name ?? "null"}");
        
        // Relics contribute to tooltip (null) and to attacks by their owner
        // null check prevents cross-character bleeding
        if (sourceCard == null || sourceCard.Owner.Creature == Owner.Creature)
        {
            decimal bonus = DynamicVars["CritHitRate"].BaseValue / 100;
            MainFile.Logger.Info($"[CommanderCap] Returning CritHitRate bonus={bonus:P0}");
            return bonus;
        }
        MainFile.Logger.Info($"[CommanderCap] sourceCard owner mismatch, returning 0");
        return 0;
    }
}