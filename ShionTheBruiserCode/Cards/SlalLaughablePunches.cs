using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 0 damage twice (3 times). If the enemy lost no HP, it loses 1 (2) Strength, and this card is Exhausted.
/// </summary>
public class SlalLaughablePunches() : SlalTheEndlessCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(0M, ValueProp.Move),
        new RepeatVar(2),
        new PowerVar<StrengthPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var attack = await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithHitCount(card.DynamicVars.Repeat.IntValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        var hpLost = attack.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (hpLost > 0 || !play.Target.IsAlive) return;

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            play.Target,
            -card.DynamicVars.Strength.BaseValue,
            card.Owner.Creature,
            card
        );
        await CardCmd.Exhaust(choiceContext, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}
