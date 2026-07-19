using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCriticalStrike() : CritContributorCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy), ICritAttackRateContributor
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14M, ValueProp.Move), 
        new CritDisplayTooltipVar(),
        new CritAttackRateVar(100)
    ];
    
    public decimal GetCritAttackRateBonus(CardModel? sourceCard) =>
        sourceCard == this && ShionMechanicsHelper.IsBerserk(sourceCard.Owner.Creature) ? DynamicVars[CritAttackRateVar.Key].BaseValue / 100 : 0m;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionCriticalStrike card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
    
    // Copied from BerserkCard
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];
    
    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
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