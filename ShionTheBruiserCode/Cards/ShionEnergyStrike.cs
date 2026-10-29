using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Turtle Hermit Shell starter. Deal 4 damage. If not Galvanised, gain 1 Vigor next turn.
/// </summary>
public class ShionEnergyStrike() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike, ShionCardTags.Energy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<VigorNextTurn>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorNextTurn>(),
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
        // Checked before the Attack resolves, since it consumes Vigor
        var wasGalvanised = ShionMechanicsHelper.IsGalvanised(card.Owner.Creature, 1);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithEnergyBlast(card, "vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (!wasGalvanised)
        {
            await PowerCmd.Apply<VigorNextTurn>(choiceContext, card.Owner.Creature, DynamicVars["VigorNextTurn"].BaseValue, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["VigorNextTurn"].UpgradeValueBy(1);
    }
}
