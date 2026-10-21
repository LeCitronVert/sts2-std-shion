using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionStellarStrike() : ShionLatentCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Latent, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CriticalDrawVar(30),
        new CalculationBaseVar(24),
        // Additional damage equal to 2x (3x upgraded) your Vigor
        new ExtraDamageVar(2M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) => model.Owner.Creature.GetPower<VigorPower>()?.Amount ?? 0),
    ];
    
    

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1M);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
}