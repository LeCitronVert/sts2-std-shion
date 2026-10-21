using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// X times (X+1 times): deal 1 damage, plus additional damage equal to the enemy's Vulnerable.
/// Double if you are Vulnerable. Double if the enemy intends to attack.
/// The spec doesn't say how X is used: here it is the number of hits.
/// </summary>
public class SeelsStrikeFirst() : SeelsTheRecklessCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override bool HasEnergyCostX => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1M, ValueProp.Move),
        new DynamicVar("Bonus", 0M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hits = ResolveEnergyXValue() + DynamicVars["Bonus"].IntValue;
        if (hits <= 0) return;

        var damage = DynamicVars.Damage.BaseValue + SeelsMechanicsHelper.GetVulnerable(play.Target);
        if (Owner.Creature.HasPower<VulnerablePower>()) damage *= 2;
        if (SeelsMechanicsHelper.IntendsToAttack(play.Target)) damage *= 2;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1M); //todo : upgrade not specified
    }
}
