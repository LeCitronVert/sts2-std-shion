using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalLaughablePunches() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3M, ValueProp.Move),
        new RepeatVar(2),
        new PowerVar<WeakPower>(2),
        new DynamicVar("SelfWeak", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars["SelfWeak"].BaseValue, card.Owner.Creature, card);

        var attack = await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithHitCount(card.DynamicVars.Repeat.IntValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        var hpLost = attack.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (hpLost > 0 || !play.Target.IsAlive) return;

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            play.Target,
            card.DynamicVars.Weak.BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(2);
    }
}
