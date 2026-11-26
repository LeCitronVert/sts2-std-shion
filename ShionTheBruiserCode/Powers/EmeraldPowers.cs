using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Ivy (Green Chaos Emerald): for (Amount) turns, the debuffs you apply to others are doubled.
/// </summary>
public sealed class EmeraldIvy : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target,
        CardModel? cardSource)
    {
        if (giver != Owner || target == null || target == Owner) return 1m;
        return power.GetTypeForAmount(amount) == PowerType.Debuff ? 2m : 1m;
    }

    public override Task AfterModifyingPowerAmountGiven(PowerModel power)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Decrement(this);
    }
}

/// <summary>
/// Slow (Yellow Chaos Emerald), on an enemy until the end of the players' turn: it takes (Amount)% more Attack damage.
/// Goes up by 10% whenever a card is played (like the base game's Slow).
/// </summary>
public sealed class EmeraldSlow : ShionTheBruiserPower
{
    public const int PerCardPlayed = 10;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // The Slow card that applied it doesn't count: it starts at its initial value
        if (Owner.IsDead || cardPlay.Card is Cards.EmeraldPowerSlow) return;
        await PowerCmd.ModifyAmount(choiceContext, this, PerCardPlayed, null, null, silent: true);
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        if (target != Owner || !props.IsPoweredAttack()) return 1m;
        return 1m + Amount / 100m;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player) await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Avatar (Blue Chaos Emerald): the next Power card you play is played (Amount) additional times.
/// </summary>
public sealed class EmeraldAvatar : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner || card.Type != CardType.Power) return playCount;
        return playCount + Amount;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Bullet (Red Chaos Emerald): this turn, your Attack damage is multiplied by (Amount).
/// </summary>
public sealed class EmeraldBullet : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        if (dealer != Owner || !props.IsPoweredAttack() || Amount <= 0) return 1m;
        return Amount;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
