using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Weak. Deal damage equal to the enemy's Weak. If Fatal, raise your Max HP by 3 (4). Exhaust.
/// Gaining Weak first makes it a Weak attack, so it deals double damage to Confused enemies and can execute them.
/// </summary>
public class SlalAbsorption() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((_, target) => target?.GetPowerAmount<WeakPower>() ?? 0),
        new MaxHpVar(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.Static(StaticHoverTip.Fatal),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        var shouldTriggerFatal = play.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        var attack = await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        // Also counts the Confusion execute, which happens right after the hit
        var killed = attack.Results.SelectMany(r => r).Any(r => r.WasTargetKilled) || play.Target.IsDead;
        if (shouldTriggerFatal && killed)
        {
            await CreatureCmd.GainMaxHp(card.Owner.Creature, card.DynamicVars.MaxHp.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(1);
    }
}
