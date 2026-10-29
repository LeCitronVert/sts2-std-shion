using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal X damage. If the enemy has Block, remove it before attacking and deal 50 additional damage.
/// </summary>
public class ShionShieldBuster() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Move),
        new DynamicVar("BonusDamage", 50),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) => GetDamage(card, target)),
        new CriticalDrawVar(20)
    ];

    private static decimal GetDamage(CardModel card, Creature? target) =>
        card.DynamicVars.Damage.BaseValue + (target is { Block: > 0 } ? card.DynamicVars["BonusDamage"].BaseValue : 0);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        CardKeyword.Retain,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var target = play.Target;
        ArgumentNullException.ThrowIfNull(target);
        
        VfxCmd.PlayOnCreatureCenter(card.Owner.Creature, "vfx/vfx_flying_slash");
        var damage = GetDamage(card, target);
        if (target.Block > 0)
        {
            await CreatureCmd.LoseBlock(target, target.Block);
        }
        await DamageCmd.Attack(damage)
            .FromCard(card)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
}