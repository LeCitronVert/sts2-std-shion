using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Not Confusion because it needs to happen immediately and shouldn't be made multi-target.
/// </summary>
public class SlalAbsorption() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HpLoss").WithMultiplier((_, target) => target?.GetPowerAmount<WeakPower>() ?? 0),
        new MaxHpVar(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.Static(StaticHoverTip.Fatal),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var hpLoss = play.Target.GetPowerAmount<WeakPower>();
        if (hpLoss <= 0) return;

        var shouldTriggerFatal = play.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        var results = await CreatureCmd.Damage(
            choiceContext,
            play.Target,
            hpLoss,
            ValueProp.Unblockable | ValueProp.Unpowered,
            card.Owner.Creature,
            card
        );

        if (shouldTriggerFatal && results.Any(r => r.WasTargetKilled))
        {
            await CreatureCmd.GainMaxHp(card.Owner.Creature, card.DynamicVars.MaxHp.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(1);
    }
}
