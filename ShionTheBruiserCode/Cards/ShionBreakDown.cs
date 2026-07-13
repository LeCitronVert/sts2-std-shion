using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBreakDown() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(3),
        new CriticalDrawVar(20),
        new CalculationBaseVar(5),
        new ExtraDamageVar(5),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) =>
            (model.Owner.Creature.GetPower<VigorPower>()?.Amount ?? 0) > 0 ? 1 : 0
        +   ShionMechanicsHelper.GetDragonUniqueBuffCount(model.Owner.Creature) >= 3 ? 1 : 0
        + (ShionMechanicsHelper.IsBerserk(model.Owner.Creature) ? 1 : 0)
        )
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionBreakDown card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .WithValueProp(ValueProp.Move)
            .WithHitCount(card.DynamicVars.Repeat.IntValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Repeat.UpgradeValueBy(2M);
    }
    
    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
        
        return Task.CompletedTask;
    }
    
    public override Task BeforeCombatStart()    
    {
        if (!Owner.Creature.HasPower<ShionCritTrackerPower>())
        {
            ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
        }
        
        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return Task.CompletedTask;
        
        ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(card.Owner.Creature);
        
        return Task.CompletedTask;
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature) || ShionMechanicsHelper.HasVigor(this.Owner.Creature) || ShionMechanicsHelper.GetDragonUniqueBuffCount(this.Owner.Creature) >= 3;
}