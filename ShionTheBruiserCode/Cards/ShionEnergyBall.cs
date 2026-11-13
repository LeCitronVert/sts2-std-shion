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
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

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
        new PowerVar<VigorNextTurn>(2)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VigorNextTurn>(),
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

        var isGalvanised = ShionMechanicsHelper.TriggerGalvanised(card, GalvanisedThreshold);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card, play)
            .Targeting(play.Target)
            .WithEnergyBlast(card, "vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (!isGalvanised) return;

        // Mid-play, so it goes through VigorNextTurn rather than VigorPower.
        await PowerCmd.Apply<VigorNextTurn>(choiceContext, card.Owner.Creature, card.DynamicVars["VigorNextTurn"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.CalculationBase.UpgradeValueBy(2M);
        this.DynamicVars.ExtraDamage.UpgradeValueBy(2M);
        this.DynamicVars["VigorNextTurn"].UpgradeValueBy(1M);
    }
}