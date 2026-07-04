using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDragonSmash() : ShionTheBruiserCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(3),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) => ShionMechanicsHelper.GetDragonUniqueBuffCount(model.Owner.Creature)),
    ];
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var xAmount = card.ResolveEnergyXValue();
        if (xAmount == 0) return;
        
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithHitCount(xAmount)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
    }
}