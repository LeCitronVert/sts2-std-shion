using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>A card whose Maim depends on a condition checked when it's played (High Reward).</summary>
public interface IConditionalMaim
{
    int ConditionalMaim { get; }
}

/// <summary>
/// A card whose Maim is applied by another player than its owner, who also gets the Blood (Cruel Shot: the Fool shoots, Seels Maims).
/// </summary>
public interface IMaimCredited
{
    /// <summary>The player who applies the Maim and gains the Blood. Null to use the card's owner.</summary>
    Player? MaimCredit { get; }
}

/// <summary>
/// Maim X: if an enemy loses HP from the card (once per attack, not per hit), it loses X Strength this turn and the card's owner gains 1 Blood (regardless of X).
/// <para>
/// A card's Maim is its native "Maim" var (cards that have Maim printed on them) plus any Maim added to it this combat
/// (Go Crazy, Steal Blood). Cards with Maim carry the <see cref="SeelsCardKeywords.Maim"/> keyword for the hover tip,
/// and <see cref="Patches.MaimDescriptionPatch"/> writes the "Maim X." line.
/// </para>
/// </summary>
public static class MaimHelper
{
    public const string VarName = "Maim";

    /// <summary>Blood gained per Maim trigger, independent of the Maim amount.</summary>
    public const int BloodGained = 1;

    /// <summary>Maim added to a card on top of its native amount. Copies of the card keep it.</summary>
    private static readonly SpireField<CardModel, int> AddedMaim = new SpireField<CardModel, int>(() => 0).CopyOnClone();

    public static DynamicVar Var(decimal amount) => new(VarName, amount);

    public static int GetNativeMaim(CardModel card) =>
        card.DynamicVars.TryGetValue(VarName, out var v) ? v.IntValue : 0;

    public static int GetAddedMaim(CardModel card) => AddedMaim.Get(card);

    public static int GetMaim(CardModel card) =>
        GetNativeMaim(card) + GetAddedMaim(card) + (card is IConditionalMaim c ? c.ConditionalMaim : 0);

    /// <summary>Whether the card counts as a "card with Maim" (including High Reward, whose Maim is conditional).</summary>
    public static bool HasMaim(CardModel card) => GetMaim(card) > 0 || card.Keywords.Contains(SeelsCardKeywords.Maim);

    /// <summary>Adds Maim X to a card (stacks with the Maim it already has).</summary>
    public static void AddMaim(CardModel card, int amount)
    {
        if (amount <= 0) return;
        AddedMaim.Set(card, AddedMaim.Get(card) + amount);
        if (!card.Keywords.Contains(SeelsCardKeywords.Maim))
        {
            card.AddKeyword(SeelsCardKeywords.Maim);
        }
        SeelsMechanicsHelper.CountModification(card);
    }

    /// <summary>
    /// Attacks currently executing, per card (a stack, since an attack can trigger another attack from the same card),
    /// each with the enemies that lost HP from it so far.
    /// Their HP loss is handled once per attack in <see cref="OnAttackFinished"/> instead of per hit.
    /// </summary>
    private static readonly Dictionary<CardModel, List<HashSet<Creature>>> AttacksInProgress = new();

    public static void OnAttackStarted(AttackCommand command)
    {
        if (command.ModelSource is not CardModel card) return;
        if (!AttacksInProgress.TryGetValue(card, out var stack))
        {
            AttacksInProgress[card] = stack = new List<HashSet<Creature>>();
        }
        stack.Add(new HashSet<Creature>());
    }

    /// <summary>
    /// Called for every damage instance. HP loss that isn't part of an attack (e.g. "an enemy loses X HP") triggers Maim right away;
    /// HP loss from an attack is recorded and triggers Maim once the attack finishes.
    /// </summary>
    public static async Task OnDamageReceived(PlayerChoiceContext choiceContext, Creature target, int hpLost, CardModel? cardSource)
    {
        if (cardSource is not null && hpLost > 0)
        {
            if (AttacksInProgress.TryGetValue(cardSource, out var stack) && stack.Count > 0)
            {
                stack[^1].Add(target);
            }
            else
            {
                await TriggerMaim(choiceContext, target, cardSource);
            }
        }

        // Killing blow: the game doesn't dispatch AfterAttack once the combat is ending (Hook.IterateCombatHookListeners
        // yields nothing), so the attacks in progress would never trigger their Maim. Trigger it now instead
        // (the Blood is held by GainBlood and granted in BloodCarryOverSingleton.AfterCombatEnd).
        if (CombatManager.Instance.IsEnding)
        {
            await FlushAttacksInProgress(choiceContext);
        }
    }

    /// <summary>
    /// Called once an attack finishes. Each enemy that lost HP from any of its hits triggers the card's Maim once.
    /// </summary>
    public static async Task OnAttackFinished(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.ModelSource is not CardModel card) return;
        if (!AttacksInProgress.TryGetValue(card, out var stack) || stack.Count == 0) return;

        var targetsThatLostHp = stack[^1];
        stack.RemoveAt(stack.Count - 1);
        if (stack.Count == 0) AttacksInProgress.Remove(card);

        foreach (var target in targetsThatLostHp.ToList())
        {
            await TriggerMaim(choiceContext, target, card);
        }
    }

    /// <summary>Triggers the Maim of every attack in progress for the enemies that lost HP from it so far (once each).</summary>
    private static async Task FlushAttacksInProgress(PlayerChoiceContext choiceContext)
    {
        foreach (var (card, stack) in AttacksInProgress.ToList())
        {
            foreach (var targets in stack)
            {
                var pending = targets.ToList();
                targets.Clear();
                foreach (var target in pending)
                {
                    await TriggerMaim(choiceContext, target, card);
                }
            }
        }
    }

    private static async Task TriggerMaim(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource)
    {
        // A credited player who died can't apply it: the card's owner does, and nobody gets the Blood.
        var credited = (cardSource as IMaimCredited)?.MaimCredit?.Creature;
        var owner = credited is { IsAlive: true } ? credited : cardSource.Owner?.Creature;
        if (owner is null || target.Side == owner.Side) return;

        var maim = GetMaim(cardSource);
        if (maim <= 0) return;

        if (target.IsAlive)
        {
            await PowerCmd.Apply<MaimStrengthDown>(choiceContext, target, maim, owner, cardSource);
        }
        if (owner.IsAlive && (credited == null || credited == owner))
        {
            await SeelsMechanicsHelper.GainBlood(choiceContext, owner, BloodGained, cardSource);
        }
    }

    /// <summary>Clears attack tracking between combats (in case an attack was interrupted).</summary>
    public static void Reset() => AttacksInProgress.Clear();
}
