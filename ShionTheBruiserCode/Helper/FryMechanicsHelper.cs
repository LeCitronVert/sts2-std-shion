using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Implemented by models that want to react to Fry's mechanics.
/// </summary>
public interface IFryListener
{
    /// <summary>Called when a Requested counter reaches 0, before its effect triggers.</summary>
    Task AfterRequestCounterReachedZero(PlayerChoiceContext choiceContext, Requested request) => Task.CompletedTask;
}

public static class FryMechanicsHelper
{
    // ── Bad Faith / Damage Debt / Refuse ────────────────────────────────────

    /// <summary>
    /// True while Damage Debt is being paid, so that this HP loss isn't turned into Damage Debt again by Bad Faith.
    /// </summary>
    public static bool IsPayingDebt { get; private set; }

    public static int GetDamageDebt(Creature creature) => creature.GetPowerAmount<DamageDebt>();

    /// <summary>
    /// Decoy Trial: Bad Faith, Refuse and Damage Debt loss granted by a card also benefit ALL other players.
    /// </summary>
    private static IEnumerable<Creature> GetBeneficiaries(Creature creature, CardModel? source)
    {
        yield return creature;
        if (source is null || !creature.HasPower<DecoyTrial>() || creature.CombatState is null) yield break;

        foreach (var ally in creature.CombatState.GetTeammatesOf(creature).Where(c => c.IsPlayer && c.IsAlive && c != creature).ToList())
        {
            yield return ally;
        }
    }

    public static async Task GainBadFaith(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if (amount <= 0) return;
        foreach (var target in GetBeneficiaries(creature, source).ToList())
        {
            await PowerCmd.Apply<BadFaith>(choiceContext, target, amount, creature, source);
        }
    }

    public static async Task Refuse(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if (amount <= 0) return;
        foreach (var target in GetBeneficiaries(creature, source).ToList())
        {
            await PowerCmd.Apply<Refuse>(choiceContext, target, amount, creature, source);
        }
    }

    public static async Task LoseDamageDebt(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if (amount <= 0) return;
        foreach (var target in GetBeneficiaries(creature, source).ToList())
        {
            var debt = target.GetPower<DamageDebt>();
            if (debt is null) continue;

            if (amount >= debt.Amount)
            {
                await PowerCmd.Remove(debt);
            }
            else
            {
                await PowerCmd.ModifyAmount(choiceContext, debt, -amount, creature, source);
            }
        }
    }

    public static Task LoseHalfDamageDebt(PlayerChoiceContext choiceContext, Creature creature, CardModel? source)
    {
        return LoseDamageDebt(choiceContext, creature, GetDamageDebt(creature) / 2, source);
    }

    public static Task RemoveDamageDebt(PlayerChoiceContext choiceContext, Creature creature, CardModel? source)
    {
        return LoseDamageDebt(choiceContext, creature, GetDamageDebt(creature), source);
    }

    /// <summary>
    /// Loses HP equal to the Damage Debt, without being caught again by Bad Faith.
    /// </summary>
    public static async Task PayDebt(PlayerChoiceContext choiceContext, Creature creature, decimal amount)
    {
        if (amount <= 0) return;

        var previous = IsPayingDebt;
        IsPayingDebt = true;
        try
        {
            await CreatureCmd.Damage(choiceContext, creature, amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
        finally
        {
            IsPayingDebt = previous;
        }
    }

    // ── Request ─────────────────────────────────────────────────────────────

    public static IReadOnlyList<Requested> GetRequests(Creature creature)
    {
        return creature.Powers.OfType<Requested>().ToList();
    }

    /// <summary>
    /// Request X: the effect triggers at the start of your turn in X turns (immediately if X is 0 or less).
    /// Summon Bigger Fish can lower X.
    /// </summary>
    public static async Task Request(PlayerChoiceContext choiceContext, CardModel card, int turns, Func<PlayerChoiceContext, Task> effect)
    {
        var creature = card.Owner.Creature;
        var biggerFish = creature.GetPower<SummonBiggerFish>();
        if (biggerFish is not null && biggerFish.TryConsume())
        {
            turns--;
        }

        if (turns <= 0)
        {
            await effect(choiceContext);
            return;
        }

        var request = (Requested)ModelDb.Power<Requested>().ToMutable();
        request.Setup(card, effect);
        await PowerCmd.Apply(choiceContext, request, creature, turns, creature, card);
    }

    /// <summary>
    /// Decrease ALL your Requested counters by X. Counters reaching 0 trigger immediately.
    /// </summary>
    public static async Task DecreaseAllRequests(PlayerChoiceContext choiceContext, Creature creature, int amount)
    {
        foreach (var request in GetRequests(creature))
        {
            await request.Advance(choiceContext, amount);
        }
    }

    // ── The Valkyrs ─────────────────────────────────────────────────────────

    /// <summary>
    /// True while the Valkyrs are dealing damage (used by effects that care about Valkyr damage).
    /// </summary>
    public static bool IsValkyrDamage { get; private set; }

    /// <summary>
    /// The Valkyrs deal damage: it is not affected by any of your modifiers (no dealer, no card source),
    /// but the target's own modifiers (like Vulnerable) still apply.
    /// </summary>
    public static async Task ValkyrsDamage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount)
    {
        var targetList = targets.Where(t => t.IsAlive).ToList();
        if (amount <= 0 || targetList.Count == 0) return;

        var previous = IsValkyrDamage;
        IsValkyrDamage = true;
        try
        {
            await CreatureCmd.Damage(choiceContext, targetList, amount, ValueProp.Move, null, null);
        }
        finally
        {
            IsValkyrDamage = previous;
        }
    }

    /// <summary>
    /// The Valkyrs grant Block: not affected by any of your modifiers.
    /// </summary>
    public static async Task ValkyrsBlock(Creature target, decimal amount)
    {
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(target, amount, ValueProp.Unpowered, null);
    }

    // ── Misc ────────────────────────────────────────────────────────────────

    public static bool IntendsToAttack(Creature? enemy)
    {
        return enemy?.Monster?.IntendsToAttack ?? false;
    }
}
