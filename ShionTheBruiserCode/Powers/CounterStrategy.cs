using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Patches;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Counter the next X hindrances (debuff, Affliction or Status card): each one grants Strength and Dexterity
/// and uses up 1 stack.
/// </summary>
public class CounterStrategy() : ShionTheBruiserPower, ICardAfflictionListener
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StatsGain", StatsGain)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public const int StatsGain = 1;

    /// <summary>
    /// Amount of each internal power (Strength, Dexterity...) that the owner's temporary buffs will take back when
    /// they expire this turn end. Those removals (e.g. Scrappy, Flex Potion) are a buff running out, not a hindrance.
    /// </summary>
    private readonly Dictionary<Type, decimal> _expiringTempBuffs = new();

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        // Same classification as vanilla Artifact: negative Strength/Dexterity count as debuffs, while a debuff
        // ticking down (negative amount on a non-negative power) reads as Buff. Defensive is your own stance.
        if (power.GetTypeForAmount(amount) != PowerType.Debuff || power.Owner != Owner || !power.IsVisible
            || power is DefensiveDebuff || ConsumeTempBuffExpiry(power, amount, applier, cardSource))
        {
            return;
        }

        await Counter();
    }

    // Temporary buffs expire during AfterSideTurnEnd (vanilla removes itself before applying the negative amount),
    // so snapshot them right before and drop the allowance once the turn end is over.
    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        _expiringTempBuffs.Clear();
        foreach (var p in Owner.Powers)
        {
            if (p is not ITemporaryPower temp || p.Type != PowerType.Buff || p.Amount <= 0) continue;

            var key = temp.InternallyAppliedPower.GetType();
            _expiringTempBuffs[key] = _expiringTempBuffs.GetValueOrDefault(key) + p.Amount;
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        _expiringTempBuffs.Clear();
        return Task.CompletedTask;
    }

    private bool ConsumeTempBuffExpiry(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount >= 0 || applier != Owner || cardSource != null) return false;
        if (!_expiringTempBuffs.TryGetValue(power.GetType(), out var remaining) || remaining < -amount) return false;

        _expiringTempBuffs[power.GetType()] = remaining + amount;
        return true;
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Type != CardType.Status || card.Owner.Creature != Owner) return;

        await Counter();
    }

    public async Task OnAfflictionApplied(AfflictionModel affliction, CardModel card)
    {
        if (card.Owner.Creature != Owner) return;

        await Counter();
    }

    private async Task Counter()
    {
        if (Amount <= 0) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            StatsGain,
            Owner,
            null
        );
        await PowerCmd.Apply<DexterityPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            StatsGain,
            Owner,
            null
        );
        await PowerCmd.Decrement(this);
    }
}
