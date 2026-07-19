using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyVolley() : ShionBerserkCard(3,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Move),
        new CalculationBaseVar(5),
        new CalculationExtraVar(1),
        new CalculatedVar("HitCount").WithMultiplier((model, _) =>
        {
            return ShionMechanicsHelper.IsBerserk(model.Owner.Creature) ? model.DynamicVars.CalculationBase.IntValue : 0;
        })
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.TriggerAnim(
            card.Owner.Creature,
            Character.ShionTheBruiser.GetEnergyCastAnimIfApplicable(card.Owner.Character),
            Character.ShionTheBruiser.GetEnergyCastDelayIfApplicable(card.Owner.Character)
        );
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithHitCount((int) ((CalculatedVar) card.DynamicVars["HitCount"]).Calculate(card.Owner.Creature))
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithAttackerAnim(Character.ShionTheBruiser.GetEnergyAnimIfApplicable(card.Owner.Character), Character.ShionTheBruiser.GetEnergyDelayIfApplicable(card.Owner.Character))
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}