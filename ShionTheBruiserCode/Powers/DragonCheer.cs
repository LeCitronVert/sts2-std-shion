using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class DragonCheer() : CritContributorPower, ICritHitRateContributor
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    public const int BaseCritRate = 20;
    public const int BerserkBonusRate = 10;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
        new DynamicVar("BaseCritRate", BaseCritRate),
        new DynamicVar("BerserkBonusRate", BerserkBonusRate)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        var critRateBonus = BaseCritRate + (ShionMechanicsHelper.IsBerserk(Owner) ? BerserkBonusRate : 0);
        
        if (sourceCard == null || sourceCard.Owner.Creature == Owner)
            return (decimal) (critRateBonus * 0.01);
        
        return 0;
    }
    
    // Berserk Power stuff
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner) return Task.CompletedTask;
        
        ChangePulse();
        return Task.CompletedTask;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ChangePulse();
        
        return Task.CompletedTask;
    }
    

    public void ChangePulse()
    {
        var shouldPulse = ShionMechanicsHelper.IsBerserk(Owner);

        if (shouldPulse)
        {
            StartPulsing();
        }
        else
        {
            StopPulsing();
        }
    }
}