using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Implemented by models that want to react to Fry's mechanics.
/// </summary>
public interface IFryListener
{
    /// <summary>Called when a Requested counter reaches 0, before its effect triggers.</summary>
    Task AfterRequestCounterReachedZero(PlayerChoiceContext choiceContext, Requested request) => Task.CompletedTask;

    /// <summary>Called each time a Requested counter ticks down (including the tick that brings it to 0).</summary>
    Task AfterRequestCounterTicked(PlayerChoiceContext choiceContext, Requested request) => Task.CompletedTask;
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
    /// Decoy Trial: Argue and Refuse also benefit ALL other players.
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
        await PowerCmd.Apply<BadFaith>(choiceContext, creature, amount, creature, source);
    }

    /// <summary>
    /// Argue X: lose up to X Damage Debt, and gain the rest of X as Bad Faith.
    /// With Decoy Trial, ALL other players Argue X too (each against their own Damage Debt).
    /// </summary>
    public static async Task Argue(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if (amount <= 0) return;
        foreach (var target in GetBeneficiaries(creature, source).ToList())
        {
            var debtLoss = Math.Min(amount, GetDamageDebt(target));
            await LoseDamageDebt(choiceContext, target, debtLoss, source);
            await GainBadFaith(choiceContext, target, amount - debtLoss, source);
        }
    }

    /// <summary>The Damage Debt Argue X would remove right now (for previews).</summary>
    public static decimal ArgueDebtLoss(Creature creature, decimal amount) => Math.Max(0, Math.Min(amount, GetDamageDebt(creature)));

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
        var debt = creature.GetPower<DamageDebt>();
        if (debt is null) return;

        if (amount >= debt.Amount)
        {
            await PowerCmd.Remove(debt);
        }
        else
        {
            await PowerCmd.ModifyAmount(choiceContext, debt, -amount, creature, source);
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
    /// </summary>
    public static Task Request(PlayerChoiceContext choiceContext, CardModel card, int turns, Func<PlayerChoiceContext, Task> effect)
    {
        return Request(choiceContext, card, [turns], effect);
    }

    /// <summary>
    /// Request X, Y and Z: one Requested counter per number, each triggering the effect when it reaches 0.
    /// Strategies Manual triggers once per Request (not per counter).
    /// </summary>
    public static async Task Request(PlayerChoiceContext choiceContext, CardModel card, IReadOnlyList<int> turns, Func<PlayerChoiceContext, Task> effect)
    {
        var creature = card.Owner.Creature;
        var manual = card.Owner.GetRelic<StrategiesManual>();
        if (manual is not null) await manual.OnRequest(choiceContext);

        foreach (var t in turns)
        {
            if (creature.IsDead) return;
            if (t <= 0)
            {
                if (!await PowerfulAndHelpless.TryReplaceRequest(choiceContext, creature))
                {
                    await effect(choiceContext);
                }
                continue;
            }

            var request = (Requested)ModelDb.Power<Requested>().ToMutable();
            request.Setup(card, effect);
            await PowerCmd.Apply(choiceContext, request, creature, t, creature, card);
        }
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
    /// The Valkyrs deal damage: it is not affected by any of your modifiers (no dealer, no card source) except
    /// Command, which adds to it. The target's own modifiers (like Vulnerable) still apply.
    /// </summary>
    public static async Task ValkyrsDamage(PlayerChoiceContext choiceContext, Creature owner, IEnumerable<Creature> targets, decimal amount)
    {
        amount += GetCommand(owner);
        var targetList = targets.Where(t => t.IsAlive).ToList();
        if (amount <= 0 || targetList.Count == 0) return;

        var previous = IsValkyrDamage;
        IsValkyrDamage = true;
        try
        {
            await CreatureCmd.Damage(choiceContext, targetList, amount, ValueProp.Move, null, null, null);
        }
        finally
        {
            IsValkyrDamage = previous;
        }
    }

    /// <summary>
    /// The Valkyrs grant Block: not affected by any of your modifiers (except Command).
    /// </summary>
    public static async Task ValkyrsBlock(Creature owner, Creature target, decimal amount)
    {
        amount += GetCommand(owner);
        if (amount <= 0) return;
        await CreatureCmd.GainBlock(target, amount, ValueProp.Unpowered, null);
    }

    /// <summary>Command: added to each instance of damage and Block the Valkyrs deal or grant for this player.</summary>
    public static int GetCommand(Creature owner) => owner.GetPowerAmount<Command>();

    public static async Task GainCommand(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? source)
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<Command>(choiceContext, creature, amount, creature, source);
    }

    // ── Misc ────────────────────────────────────────────────────────────────

    public static bool IntendsToAttack(Creature? enemy)
    {
        return enemy?.Monster?.IntendsToAttack ?? false;
    }
}
