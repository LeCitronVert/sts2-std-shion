using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Fry: the partner deals 9 damage, plus Fry's Stars and the partner's number of unique buffs.
/// Both you and your partner's Attack modifiers apply for this card (same approach as <see cref="ShionDualStrike"/>).
/// </summary>
public class FryDecapitationStrike() : FryTeamUpCard(2,
    CardType.Attack,
    TargetType.AnyEnemy), ICritHitRateContributor
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(9),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => GetBonus((TeamUpCard)card)),
        new CritDisplayTooltipVar(),
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    /// <summary>Fry's Stars plus the partner's unique buffs. On Fry's own card, only his Stars are known.</summary>
    private static int GetBonus(TeamUpCard card)
    {
        if (card.Initiator is { } initiator)
        {
            return (initiator.PlayerCombatState?.Stars ?? 0) + ShionMechanicsHelper.GetDragonUniqueBuffCount(card.Owner.Creature);
        }

        return card.Owner.PlayerCombatState?.Stars ?? 0;
    }

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await TeamUpAttackHelper.EnsureCritTracker(initiator, partner);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage.Calculate(play.Target))
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
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
        EnergyCost.UpgradeBy(-1);
    }
}
