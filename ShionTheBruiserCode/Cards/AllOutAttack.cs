using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Generic Team Blast. Deal 50 damage (upgraded: 60 damage to ALL enemies). ALL players' Attack modifiers apply.
/// You deal the damage; the other players' damage modifiers and Critical Hit chance are added on top of yours
/// (same approach as <see cref="ShionDualStrike"/>).
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public class AllOutAttack() : TeamBlastCard(CardType.Attack, TargetType.AnyEnemy), ICritHitRateContributor
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(50M, ValueProp.Move)];

    public override TargetType TargetType => IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        if (!IsUpgraded) ArgumentNullException.ThrowIfNull(play.Target);

        // Crits need the tracker if anyone on the team can crit
        if (LivingPlayers.Any(p => p.Creature.HasPower<ShionCritTrackerPower>()))
        {
            await ShionMechanicsHelper.EnsureCreatureHasCritTracker(Owner.Creature);
        }

        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this);
        attack = IsUpgraded ? attack.TargetingAllOpponents(CombatState) : attack.Targeting(play.Target!);
        await attack
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    private IEnumerable<Creature> OtherPlayers =>
        CombatState?.GetTeammatesOf(Owner.Creature).Where(c => c.IsPlayer && c.IsAlive && c != Owner.Creature) ?? [];

    // Other players' modifiers (Strength, Vigor, relics...) only apply to their own attacks, so they're asked here as if each of them dealt this one.
    private IEnumerable<(Creature Player, AbstractModel Model)> OtherModifiers(Creature? dealer, CardModel? cardSource)
    {
        if (cardSource != this || dealer != Owner.Creature) return [];
        return OtherPlayers.SelectMany(p => p.Powers.Cast<AbstractModel>()
            .Concat(p.Player!.Relics.Where(r => !r.IsMelted))
            .Select(m => (p, m)));
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return OtherModifiers(dealer, cardSource)
            .Sum(x => x.Model.ModifyDamageAdditive(target, amount, props, x.Player, cardSource));
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return OtherModifiers(dealer, cardSource)
            .Aggregate(1m, (multiplier, x) => multiplier * x.Model.ModifyDamageMultiplicative(target, amount, props, x.Player, cardSource));
    }

    // Crit contributors only count their owner's attacks, so the other players' are asked for their general bonus.
    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard != this) return 0m;
        return OtherModifiers(Owner.Creature, this).Select(x => x.Model).OfType<ICritHitRateContributor>().Sum(c => c.GetCritHitRateBonus(null));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10M);
    }
}
