using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class CriticalDrawPower : CritContributorPower, ICritHitRateContributor
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
    ];
    
    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        // Powers contribute to tooltip (null) and to attacks by their owner
        if (sourceCard == null || sourceCard.Owner.Creature == Owner)
            return Math.Min(Amount, 50) * 0.01M;
        
        return 0;
    }
}