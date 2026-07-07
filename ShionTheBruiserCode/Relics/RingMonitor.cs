using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class RingMonitor() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;

    public const int BlockChance = 20;
    public const int BlockAmount = 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockChance", BlockChance),
        new DynamicVar("BlockAmount", BlockAmount)
    ];

    public override Decimal ModifyHpLostAfterOsty(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature) return amount;
        
        var roll = Owner.RunState.Rng.Niche.NextFloat();
        var odds = BlockChance / 100f;
        
        var isBlocked = roll < odds;
        
        if (isBlocked) Flash();
        
        return isBlocked ? Math.Max(0, amount - BlockAmount) : amount;
    }
}