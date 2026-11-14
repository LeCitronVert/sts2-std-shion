using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Silent: deal 6 (9) damage, and apply Poison equal to the HP the enemy lost.
/// Both you and your partner's Attack modifiers apply for this card (same approach as <see cref="ShionDualStrike"/>).
/// </summary>
public class SilentViciousStrike() : SilentTeamUpCard(1,
    CardType.Attack,
    TargetType.AnyEnemy), ICritHitRateContributor
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6M, ValueProp.Move),
        new CritDisplayTooltipVar(),
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await TeamUpAttackHelper.EnsureCritTracker(initiator, partner);

        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var hpLost = attack.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (hpLost > 0 && play.Target.IsAlive)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, play.Target, hpLost, Owner.Creature, this);
        }
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
#if STS2_BETA
        , CardPlay? cardPlay
#endif
    )
    {
#if STS2_BETA
        return TeamUpAttackHelper.ModifyDamageAdditive(this, target, amount, props, dealer, cardSource, cardPlay);
#else
        return TeamUpAttackHelper.ModifyDamageAdditive(this, target, amount, props, dealer, cardSource);
#endif
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
#if STS2_BETA
        , CardPlay? cardPlay
#endif
    )
    {
#if STS2_BETA
        return TeamUpAttackHelper.ModifyDamageMultiplicative(this, target, amount, props, dealer, cardSource, cardPlay);
#else
        return TeamUpAttackHelper.ModifyDamageMultiplicative(this, target, amount, props, dealer, cardSource);
#endif
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard) => TeamUpAttackHelper.GetCritHitRateBonus(this, sourceCard);

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
    }
}
