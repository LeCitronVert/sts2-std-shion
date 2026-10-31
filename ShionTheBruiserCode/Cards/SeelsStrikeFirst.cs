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
using ShionTheBruiser.ShionTheBruiserCode.Variables;

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
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
            target is null ? 0 : ((SeelsStrikeFirst)card).GetHitDamage(target)),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((card, _) => ((SeelsStrikeFirst)card).PreviewHits()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    private int PreviewHits() => PreviewHelper.XValue(this) + DynamicVars["Bonus"].IntValue;

    /// <summary>Damage of each hit against this enemy, before damage modifiers.</summary>
    private decimal GetHitDamage(Creature target)
    {
        var damage = DynamicVars.Damage.BaseValue + SeelsMechanicsHelper.GetVulnerable(target);
        if (Owner.Creature.HasPower<VulnerablePower>()) damage *= 2;
        if (SeelsMechanicsHelper.IntendsToAttack(target)) damage *= 2;
        return damage;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hits = ResolveEnergyXValue() + DynamicVars["Bonus"].IntValue;
        if (hits <= 0) return;

        await DamageCmd.Attack(GetHitDamage(play.Target))
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
