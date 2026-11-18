using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone): deal 16 damage. Both you and your partner's Attack modifiers apply for this card.
/// The partner deals the damage; the Initiator's damage modifiers and Critical Hit chance are added on top of theirs.
/// </summary>
public class ShionDualStrike() : ShionTeamUpCard(1,
    CardType.Attack,
    TargetType.AnyEnemy), ICritHitRateContributor
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16M, ValueProp.Move),
        new CritDisplayTooltipVar(),
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await TeamUpAttackHelper.EnsureCritTracker(initiator, partner);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        return TeamUpAttackHelper.ModifyDamageAdditive(this, target, amount, props, dealer, cardSource, cardPlay);
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        return TeamUpAttackHelper.ModifyDamageMultiplicative(this, target, amount, props, dealer, cardSource, cardPlay);
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard) => TeamUpAttackHelper.GetCritHitRateBonus(this, sourceCard);

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8M);
    }
}
