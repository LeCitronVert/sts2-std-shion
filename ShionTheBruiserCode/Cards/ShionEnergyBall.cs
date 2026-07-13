using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyBall() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(7),
        new ExtraDamageVar(5),
        // If player has Vigor, 1 x ExtraDamage / Otherwise, 0
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) => ShionMechanicsHelper.HasVigor(model.Owner.Creature) ? 1 : 0),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.HasVigor(this.Owner.Creature);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionEnergyBall card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithAttackerAnim(Character.ShionTheBruiser.GetEnergyAnimIfApplicable(card.Owner.Character), Character.ShionTheBruiser.GetEnergyDelayIfApplicable(card.Owner.Character))
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.CalculationBase.UpgradeValueBy(2M);
        this.DynamicVars.ExtraDamage.UpgradeValueBy(2M);
    }
}