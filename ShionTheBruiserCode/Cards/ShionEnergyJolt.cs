using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyJolt() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Unpowered | ValueProp.Move),
        new DynamicVar("DebuffAmount", 2)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target, "play.Target");
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            play.Target,
            DynamicVars["DebuffAmount"].BaseValue,
            card.Owner.Creature,
            card
        );
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            play.Target,
            DynamicVars["DebuffAmount"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DebuffAmount"].UpgradeValueBy(1);
    }
}