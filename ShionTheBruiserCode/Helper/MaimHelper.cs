using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    /// Cards whose attack is currently executing (counted, since an attack can trigger another attack from the same card).
    /// Their HP loss is handled once per attack in <see cref="OnAttackFinished"/> instead of per hit.
    /// </summary>
    private static readonly Dictionary<CardModel, int> AttacksInProgress = new();

    public static void OnAttackStarted(AttackCommand command)
    {
        if (command.ModelSource is not CardModel card) return;
        AttacksInProgress[card] = AttacksInProgress.GetValueOrDefault(card) + 1;
    }

    /// <summary>
    /// Called for every damage instance. Only handles HP loss that isn't part of an attack (e.g. "an enemy loses X HP").
    /// </summary>
    public static async Task OnDamageReceived(PlayerChoiceContext choiceContext, Creature target, int hpLost, CardModel? cardSource)
    {
        if (cardSource is null || hpLost <= 0) return;
        if (AttacksInProgress.ContainsKey(cardSource)) return;
        await TriggerMaim(choiceContext, target, cardSource);
    }

    /// <summary>
    /// Called once an attack finishes. Each enemy that lost HP from any of its hits triggers the card's Maim once.
    /// </summary>
    public static async Task OnAttackFinished(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.ModelSource is not CardModel card) return;
        if (AttacksInProgress.TryGetValue(card, out var count))
        {
            if (count <= 1) AttacksInProgress.Remove(card);
            else AttacksInProgress[card] = count - 1;
        }

        var targetsThatLostHp = command.Results
            .SelectMany(r => r)
            .Where(r => r.UnblockedDamage > 0)
            .Select(r => r.Receiver)
            .Distinct()
            .ToList();

        foreach (var target in targetsThatLostHp)
        {
            await TriggerMaim(choiceContext, target, card);
        }
    }

    private static async Task TriggerMaim(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource)
    {
        var owner = cardSource.Owner?.Creature;
        if (owner is null || target.Side == owner.Side) return;

        var maim = GetMaim(cardSource);
        if (maim <= 0) return;

        if (target.IsAlive)
        {
            await PowerCmd.Apply<MaimStrengthDown>(choiceContext, target, maim, owner, cardSource);
        }
        if (owner.IsAlive)
        {
            await SeelsMechanicsHelper.GainBlood(choiceContext, owner, BloodGained, cardSource);
        }
    }

    /// <summary>Clears attack tracking between combats (in case an attack was interrupted).</summary>
    public static void Reset() => AttacksInProgress.Clear();
}
