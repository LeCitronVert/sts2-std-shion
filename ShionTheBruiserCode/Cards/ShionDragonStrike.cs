using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDragonStrike() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(6M),
        new ExtraDamageVar(2),
        // Damage is doubled if owner has 3+ unique buffs
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, creature) => 3 <= ShionMechanicsHelper.GetDragonUniqueBuffCount(model.Owner.Creature) ? model.DynamicVars.CalculationBase.BaseValue : 0),
        new DamageCapVar(15)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

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
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.CalculationBase.UpgradeValueBy(3M);
        this.DynamicVars[DamageCapVar.Key].UpgradeValueBy(15M);
    }
}