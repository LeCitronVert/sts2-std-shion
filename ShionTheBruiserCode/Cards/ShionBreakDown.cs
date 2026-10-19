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

public class ShionBreakDown() : ShionBerserkCard(1,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    public const int GalvanisedThreshold = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new RepeatVar(3),
        new CriticalDrawVar(20),
        new CalculationBaseVar(5),
        new ExtraDamageVar(5),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) =>
            (ShionMechanicsHelper.IsGalvanised(model.Owner.Creature, GalvanisedThreshold) ? 1 : 0)
        +   (ShionMechanicsHelper.GetDragonUniqueBuffCount(model.Owner.Creature) >= 3 ? 1 : 0)
        )
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

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
        
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, 1, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Repeat.UpgradeValueBy(2M);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature) || ShionMechanicsHelper.IsGalvanised(this.Owner.Creature, GalvanisedThreshold) || ShionMechanicsHelper.GetDragonUniqueBuffCount(this.Owner.Creature) >= 3;
}