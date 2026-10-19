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
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyBall() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public const int GalvanisedThreshold = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(7),
        new ExtraDamageVar(5),
        // Galvanised 1: 1 x ExtraDamage / Otherwise, 0
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) => ShionMechanicsHelper.IsGalvanised(model.Owner.Creature, GalvanisedThreshold) ? 1 : 0),
        new DynamicVar("VigorRefundPercent", 33),
        new VigorRefundVar()
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsGalvanised(this.Owner.Creature, GalvanisedThreshold);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionEnergyBall card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var vigorAmount = card.Owner.Creature.GetPowerAmount<VigorPower>();
        var isGalvanised = ShionMechanicsHelper.IsGalvanised(card.Owner.Creature, GalvanisedThreshold);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithAttackerAnim(Character.ShionTheBruiser.GetEnergyAnimIfApplicable(card.Owner.Character), Character.ShionTheBruiser.GetEnergyDelayIfApplicable(card.Owner.Character))
            .Execute(choiceContext);

        if (!isGalvanised || vigorAmount <= 0) return;
        
        var refundAmount = CalculateVigorRefund(card, vigorAmount);
        
        if (refundAmount <= 0) return;
        
        await PowerCmd.Apply<VigorPower>(choiceContext, card.Owner.Creature, refundAmount, card.Owner.Creature, null);
    }

    /// <summary>
    /// Vigor that would be refunded if this card were played right now (0 when not Galvanised).
    /// </summary>
    public static decimal GetVigorRefund(CardModel card)
    {
        if (!ShionMechanicsHelper.IsGalvanised(card.Owner.Creature, GalvanisedThreshold)) return 0;
        return CalculateVigorRefund(card, card.Owner.Creature.GetPowerAmount<VigorPower>());
    }

    private static decimal CalculateVigorRefund(CardModel card, decimal vigorAmount) =>
        Math.Floor(vigorAmount * card.DynamicVars["VigorRefundPercent"].BaseValue / 100);

    protected override void OnUpgrade()
    {
        this.DynamicVars.CalculationBase.UpgradeValueBy(2M);
        this.DynamicVars.ExtraDamage.UpgradeValueBy(2M);
    }
}