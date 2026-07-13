using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public abstract class ShionBerserkCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ShionTheBruiserCard(cost, type, rarity, target)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];
    
    public override Task BeforeCombatStart()    
    {
        ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
        
        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return Task.CompletedTask;
        
        ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(card.Owner.Creature);
        
        return Task.CompletedTask;
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature);
}