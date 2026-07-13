using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyJolt() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move),
        new DynamicVar("DebuffAmount", 2)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

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
        await CreatureCmd.Damage(
            choiceContext, 
            play.Target, 
            DynamicVars.Damage, 
            card
        );
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