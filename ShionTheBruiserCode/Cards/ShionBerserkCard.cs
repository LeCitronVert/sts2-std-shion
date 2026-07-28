using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
    }
    
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return;
        
        await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(card.Owner.Creature);
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature);
}