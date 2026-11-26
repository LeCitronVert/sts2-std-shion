using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>The Emerald wheel's section icons (images/ui/emerald_wheel), reused as the Emerald powers' icons.</summary>
internal static class EmeraldPowerIcons
{
    public static string Path(string name) =>
        System.IO.Path.Join(MainFile.ResPath, "images", "ui", "emerald_wheel", name + "_available.png");
}

/// <summary>
/// Ivy (Green Chaos Emerald): for (Amount) turns, the debuffs you apply to others are doubled.
/// </summary>
public sealed class EmeraldIvy : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Same icon as its Emerald wheel section
    public override string CustomPackedIconPath => EmeraldPowerIcons.Path("ivy");
    public override string CustomBigIconPath => EmeraldPowerIcons.Path("ivy");

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
/// Slow (Yellow Chaos Emerald): hidden marker for the base game's <see cref="SlowPower"/> that the Slow Emerald Power added.
/// <para>Base Slow's Amount works as a count of its sources (Bygone Effigy's own, Extreme Speed's, this one): Amount = how many
/// Slow stacks this Emerald added, taken back off at the end of the players' turn. Slow is only removed once no other
/// source is left. Its % (the hidden SlowAmount counter) resets when the enemy's turn starts anyway.</para>
/// </summary>
public sealed class EmeraldSlow : ShionTheBruiserPower
{
    // Shown through the base game's Slow; None so it isn't a debuff of its own (Artifact, Ivy...)
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => false;

    public override string CustomPackedIconPath => ModelDb.Power<SlowPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<SlowPower>().ResolvedBigIconPath;

    private const string SlowCounterKey = "SlowAmount";
    private const string SlowDisplayKey = "DisplayAmount";

    private static readonly MethodInfo RefreshDisplayAmount =
        AccessTools.Method(typeof(PowerModel), "InvokeDisplayAmountChanged");

    /// <summary>
    /// Applies the base game's Slow to <paramref name="target"/> and raises its % by <paramref name="startingPercent"/>
    /// (minus the 10% the playing card itself adds once its play finishes).
    /// </summary>
    public static async Task Apply(PlayerChoiceContext choiceContext, Creature target, decimal startingPercent,
        Creature applier, CardModel cardSource)
    {
        var before = target.GetPowerAmount<SlowPower>();
        await PowerCmd.Apply<SlowPower>(choiceContext, target, 1m, applier, cardSource);
        var slow = target.GetPower<SlowPower>();
        var added = (slow?.Amount ?? 0) - before;
        if (slow == null || added <= 0) return; // Blocked (Artifact...)

        // Amount multipliers (Ivy) scale the starting % too
        var steps = startingPercent / 10m * added - 1m;
        var counter = slow.DynamicVars[SlowCounterKey];
        counter.BaseValue = Math.Max(0m, counter.BaseValue + steps);
        slow.DynamicVars[SlowDisplayKey].BaseValue = counter.BaseValue * 10m;
        RefreshDisplayAmount.Invoke(slow, null);

        await PowerCmd.Apply<EmeraldSlow>(choiceContext, target, added, applier, cardSource);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;

        var slow = Owner.GetPower<SlowPower>();
        if (slow != null)
        {
            // Only take back what this Emerald added: other sources (Extreme Speed, Bygone Effigy) keep theirs
            if (slow.Amount <= Amount) await PowerCmd.Remove(slow);
            else await PowerCmd.ModifyAmount(choiceContext, slow, -Amount, null, null, silent: true);
        }
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Avatar (Blue Chaos Emerald): the next Power card you play is played (Amount) additional times.
/// </summary>
public sealed class EmeraldAvatar : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Same icon as its Emerald wheel section
    public override string CustomPackedIconPath => EmeraldPowerIcons.Path("avatar");
    public override string CustomBigIconPath => EmeraldPowerIcons.Path("avatar");

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

    // Same icon as its Emerald wheel section
    public override string CustomPackedIconPath => EmeraldPowerIcons.Path("bullet");
    public override string CustomBigIconPath => EmeraldPowerIcons.Path("bullet");

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
