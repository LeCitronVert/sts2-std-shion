using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyVolley() : ShionTheBruiserCard(3,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Move),
        new RepeatVar(10),
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
            .WithHitCount(card.DynamicVars.Repeat.IntValue)
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