using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBreakDown() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5M, ValueProp.Move),
        new RepeatVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionBreakDown card = this;
        ArgumentNullException.ThrowIfNull(play.Target, "play.Target");
        AttackCommand attackCommand = await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .WithHitCount(card.DynamicVars.Repeat.IntValue)
                .FromCard(card)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext)
            ;
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Repeat.UpgradeValueBy(2M);
    }
}