using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Team-Up attacks where "both you and your partner's Attack modifiers apply" (Dual Strike, Vicious Strike).
/// The Partner deals the damage from their copy; the Initiator's damage modifiers and Critical Hit chance are added on top.
/// The card forwards its ModifyDamage* and GetCritHitRateBonus overrides here.
/// </summary>
public static class TeamUpAttackHelper
{
    /// <summary>Crits need the tracker on the Partner if the Initiator can crit.</summary>
    public static async Task EnsureCritTracker(Player initiator, Player partner)
    {
        if (initiator.Creature.HasPower<ShionCritTrackerPower>())
        {
            await ShionMechanicsHelper.EnsureCreatureHasCritTracker(partner.Creature);
        }
    }

    // The Initiator's own modifiers (Strength, Vigor, relics...) only apply to their attacks, so they're asked here as if they dealt this one.
    private static IEnumerable<AbstractModel> InitiatorModifiers(TeamUpCard card, Creature? dealer, CardModel? cardSource)
    {
        if (cardSource != card || card.Initiator is not { } initiator || dealer != card.Owner.Creature) return [];
        return [..initiator.Creature.Powers, ..initiator.Relics.Where(r => !r.IsMelted)];
    }

    public static decimal ModifyDamageAdditive(TeamUpCard card, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
#if STS2_BETA
        , CardPlay? cardPlay
#endif
    )
    {
        return InitiatorModifiers(card, dealer, cardSource)
#if STS2_BETA
            .Sum(m => m.ModifyDamageAdditive(target, amount, props, card.Initiator!.Creature, cardSource, cardPlay));
#else
            .Sum(m => m.ModifyDamageAdditive(target, amount, props, card.Initiator!.Creature, cardSource));
#endif
    }

    public static decimal ModifyDamageMultiplicative(TeamUpCard card, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
#if STS2_BETA
        , CardPlay? cardPlay
#endif
    )
    {
        return InitiatorModifiers(card, dealer, cardSource)
#if STS2_BETA
            .Aggregate(1m, (multiplier, m) => multiplier * m.ModifyDamageMultiplicative(target, amount, props, card.Initiator!.Creature, cardSource, cardPlay));
#else
            .Aggregate(1m, (multiplier, m) => multiplier * m.ModifyDamageMultiplicative(target, amount, props, card.Initiator!.Creature, cardSource));
#endif
    }

    // Crit contributors only count their owner's attacks, so the Initiator's are asked for their general bonus.
    public static decimal GetCritHitRateBonus(TeamUpCard card, CardModel? sourceCard)
    {
        if (sourceCard != card || card.Initiator is null) return 0m;
        return InitiatorModifiers(card, card.Owner.Creature, card).OfType<ICritHitRateContributor>().Sum(c => c.GetCritHitRateBonus(null));
    }
}
