using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class CritAttackUp : CritContributorPower, ICritAttackRateContributor
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
    ];
    
    public decimal GetCritAttackRateBonus(CardModel? sourceCard)
    {
        if (sourceCard == null || sourceCard.Owner.Creature == Owner)
            return (decimal)(this.Amount * 0.01);
        
        return 0;
    }
}