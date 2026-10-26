using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Purple challenge (on each enemy): can't take more than 12 damage from a single card play, -1 per extra stack.
/// </summary>
public class PurpleEmeraldChallenge : EmeraldChallengePower
{
    public const int BaseCap = 12;

    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Purple;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("DamageCap", BaseCap)];

    private readonly Dictionary<CardModel, decimal> _damageTakenFromCard = [];

    private decimal Cap => Math.Max(1, BaseCap - (Amount - 1));

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["DamageCap"].BaseValue = Cap;
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this) DynamicVars["DamageCap"].BaseValue = Cap;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || cardSource is null) return decimal.MaxValue;
        return Math.Max(0, Cap - _damageTakenFromCard.GetValueOrDefault(cardSource));
    }

    // Enemies summoned mid-fight are bound too. Only the first enemy holding the challenge passes it on.
    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (!creature.IsEnemy || creature.HasPower<PurpleEmeraldChallenge>()) return;
        if (CombatState.Enemies.FirstOrDefault(e => e.HasPower<PurpleEmeraldChallenge>()) != Owner) return;
        await PowerCmd.Apply<PurpleEmeraldChallenge>(new BlockingPlayerChoiceContext(), creature, Amount, null, null);
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        _damageTakenFromCard.Remove(cardPlay.Card);
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && cardSource is not null)
        {
            _damageTakenFromCard[cardSource] = _damageTakenFromCard.GetValueOrDefault(cardSource) + result.TotalDamage;
        }
        return Task.CompletedTask;
    }
}
