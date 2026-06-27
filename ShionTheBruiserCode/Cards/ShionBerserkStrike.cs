using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBerserkStrike() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(9),
        new ExtraDamageVar(34),
        // Damage scales with missing hp, up until the maximum which is stored in ExtraDamageVar
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, creature) => 
            (model.DynamicVars.ExtraDamage.BaseValue - model.DynamicVars.CalculationBase.BaseValue)
            * (1 - (decimal)model.Owner.Creature.CurrentHp / model.Owner.Creature.MaxHp)
        ),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target, "play.Target");
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(18);
    }
}