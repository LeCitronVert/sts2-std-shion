using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

// All your Attacks deal Critical Hits.
// Galvanised effects no longer apply (see ShionMechanicsHelper.IsGalvanised)
public class SpiritBreak : CritContributorPower, ICritHitRateContributor
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard == null) return 1;
        
        return sourceCard.Owner.Creature == Owner && sourceCard.Type == CardType.Attack ? 1 : 0;    
    }
}
