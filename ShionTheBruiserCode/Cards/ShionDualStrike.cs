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

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (initiator.Creature.HasPower<ShionCritTrackerPower>())
        {
            await ShionMechanicsHelper.EnsureCreatureHasCritTracker(partner.Creature);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    // The Initiator's own modifiers (Strength, Vigor, relics...) only apply to their attacks, so they're asked here as if they dealt this one.
    private IEnumerable<AbstractModel> InitiatorModifiers(Creature? dealer, CardModel? cardSource)
    {
        if (cardSource != this || Initiator is not { } initiator || dealer != Owner.Creature) return [];
        return [..initiator.Creature.Powers, ..initiator.Relics.Where(r => !r.IsMelted)];
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return InitiatorModifiers(dealer, cardSource)
            .Sum(m => m.ModifyDamageAdditive(target, amount, props, Initiator!.Creature, cardSource));
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return InitiatorModifiers(dealer, cardSource)
            .Aggregate(1m, (multiplier, m) => multiplier * m.ModifyDamageMultiplicative(target, amount, props, Initiator!.Creature, cardSource));
    }

    // Crit contributors only count their owner's attacks, so the Initiator's are asked for their general bonus.
    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard != this || Initiator is not { } initiator) return 0m;
        return InitiatorModifiers(Owner.Creature, this).OfType<ICritHitRateContributor>().Sum(c => c.GetCritHitRateBonus(null));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8M);
    }
}
