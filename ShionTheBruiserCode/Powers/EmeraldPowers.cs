using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Patches;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>The Emerald wheel's section icons (images/ui/emerald_wheel), reused as the Emerald powers' icons.</summary>
internal static class EmeraldPowerIcons
{
    public static string Path(string name) =>
        System.IO.Path.Join(MainFile.ResPath, "images", "ui", "emerald_wheel", name + "_available.png");
}

// ═════════════════════════════════════════════════════════════════════════════
//  Passives: each held Chaos Emerald applies its passive at the start of every fight (ChaosEmeraldRelic).
//  Playing that emerald's Emerald Power (its Burst) turns the passive off from the start of your next turn.
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A Chaos Emerald's passive (applied by <see cref="Relics.ChaosEmeraldRelic"/> at the start of each fight). Playing its
/// Emerald Power (<see cref="EmeraldPowerCard"/>) calls <see cref="TurnOffNextTurn"/>: it stays on for the rest of this
/// turn, then is removed at the start of the owner's next turn.
/// </summary>
public abstract class EmeraldPassive : ShionTheBruiserPower
{
    public const string EndingKey = "Ending";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public abstract ChaosEmeraldColor Color { get; }
    protected abstract string IconName { get; }

    // Same icon as its Emerald wheel section
    public override string CustomPackedIconPath => EmeraldPowerIcons.Path(IconName);
    public override string CustomBigIconPath => EmeraldPowerIcons.Path(IconName);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(EndingKey, 0)];

    public bool Ending => DynamicVars[EndingKey].BaseValue > 0;

    public void TurnOffNextTurn()
    {
        AssertMutable();
        DynamicVars[EndingKey].BaseValue = 1;
        Flash();
        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Ending && participants.Contains(Owner)) await PowerCmd.Remove(this);
    }

    public static EmeraldPassive? Get(Creature creature, ChaosEmeraldColor color) =>
        creature.Powers.OfType<EmeraldPassive>().FirstOrDefault(p => p.Color == color);

    /// <summary>Applies that color's passive (once: it doesn't stack).</summary>
    public static async Task Apply(PlayerChoiceContext choiceContext, Creature creature, ChaosEmeraldColor color)
    {
        if (Get(creature, color) is not null) return;
        switch (color)
        {
            case ChaosEmeraldColor.Cyan: await PowerCmd.Apply<EmeraldWater>(choiceContext, creature, 1m, creature, null); break;
            case ChaosEmeraldColor.Purple: await PowerCmd.Apply<EmeraldVision>(choiceContext, creature, 1m, creature, null); break;
            case ChaosEmeraldColor.Green: await PowerCmd.Apply<EmeraldIvy>(choiceContext, creature, 1m, creature, null); break;
            case ChaosEmeraldColor.Yellow: await PowerCmd.Apply<EmeraldSlow>(choiceContext, creature, 1m, creature, null); break;
            case ChaosEmeraldColor.Blue: await PowerCmd.Apply<EmeraldAvatar>(choiceContext, creature, 1m, creature, null); break;
            case ChaosEmeraldColor.Red: await PowerCmd.Apply<EmeraldBullet>(choiceContext, creature, 1m, creature, null); break;
            default: await PowerCmd.Apply<EmeraldExtra>(choiceContext, creature, 1m, creature, null); break;
        }
    }

    /// <summary>The Emerald Power cards themselves never trigger "first card / Power you play" passives.</summary>
    protected static bool IsEmeraldPower(CardModel card) => card is EmeraldPowerCard;
}

/// <summary>
/// Water passive (Cyan): the first Power you play from your Hand each turn adds an Ethereal copy of itself to your Hand
/// (Emerald Powers are skipped and don't use it up).
/// </summary>
public sealed class EmeraldWater : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;
    protected override string IconName => "water";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

    private bool _usedThisTurn;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (_usedThisTurn || !cardPlay.IsFirstInSeries || card.Owner.Creature != Owner || card.Type != CardType.Power ||
            IsEmeraldPower(card) || CardAutoPlayTracker.IsBeingAutoPlayed(card)) return;
        if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.CombatState is not { } combatState) return;
        _usedThisTurn = true;
        Flash();
        var copy = combatState.CloneCard(card);
        if (!copy.Keywords.Contains(CardKeyword.Ethereal)) copy.AddKeyword(CardKeyword.Ethereal);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, card.Owner);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner)) _usedThisTurn = false;
        return Task.CompletedTask;
    }
}

/// <summary>Vision passive (Purple): Attack damage you take from enemies is reduced by 3 (before Vulnerable etc.).</summary>
public sealed class EmeraldVision : EmeraldPassive
{
    public const decimal Reduction = 3m;

    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Purple;
    protected override string IconName => "vision";

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        if (target != Owner || dealer == null || dealer.Side == Owner.Side || !props.IsPoweredAttack() || amount <= 0) return 0m;
        return -Math.Min(Reduction, amount);
    }
}

/// <summary>Ivy passive (Green): whenever you play a Skill, apply 1 Poison to a random enemy.</summary>
public sealed class EmeraldIvy : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Green;
    protected override string IconName => "ivy";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Skill) return;
        if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.CombatState is not { } combatState) return;
        if (Owner.Player is not { } player) return;
        var target = player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
        if (target == null) return;
        Flash();
        await PowerCmd.Apply<PoisonPower>(choiceContext, target, 1m, Owner, null);
    }
}

/// <summary>
/// Slow passive (Yellow): whenever you deal unblocked Attack damage, gain Rings equal to half of it (rounded down, per
/// hit).
/// </summary>
public sealed class EmeraldSlow : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;
    protected override string IconName => "slow";

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || Owner.IsDead || !props.IsPoweredAttack() || target.Side == Owner.Side) return;
        var rings = result.UnblockedDamage / 2;
        if (rings <= 0) return;
        Flash();
        await Rings.Gain(choiceContext, Owner, rings);
    }
}

/// <summary>Avatar passive (Blue): at the end of your turn, gain 1 energy next turn for each card in your Hand.</summary>
public sealed class EmeraldAvatar : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Blue;
    protected override string IconName => "avatar";

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        var cards = PileType.Hand.GetPile(player).Cards.Count;
        if (cards <= 0) return;
        Flash();
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, cards, Owner, null);
    }
}

/// <summary>Bullet passive (Red): whenever you attack a Vulnerable enemy, heal 1 (per Vulnerable enemy hit).</summary>
public sealed class EmeraldBullet : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Red;
    protected override string IconName => "bullet";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner || Owner.IsDead) return;
        var vulnerable = command.Results.SelectMany(r => r).Select(r => r.Receiver)
            .Distinct().Count(c => c.Side != Owner.Side && c.HasPower<VulnerablePower>());
        if (vulnerable <= 0) return;
        Flash();
        await CreatureCmd.Heal(Owner, vulnerable);
    }
}

/// <summary>
/// Extra passive (Grey): at the end of your turn, a random card in your Discard Pile without Replay gains Replay 1 (for
/// the rest of the fight). Runs before the Hand is discarded.
/// </summary>
public sealed class EmeraldExtra : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;
    protected override string IconName => "extra";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.ReplayStatic)];

    public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || Owner.IsDead || CombatManager.Instance.IsOverOrEnding) return Task.CompletedTask;
        var options = PileType.Discard.GetPile(player).Cards.Where(c => c.BaseReplayCount <= 0).ToList();
        if (options.Count == 0) return Task.CompletedTask;
        var card = player.RunState.Rng.CombatCardSelection.NextItem(options);
        if (card == null) return Task.CompletedTask;
        Flash();
        card.BaseReplayCount += 1;
        return Task.CompletedTask;
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  Bursts with a lasting part (applied by the Emerald Power cards). Same icon as their emerald.
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>A temporary Burst effect, removed at the end of its owner's turn (or after Amount turns for counters).</summary>
public abstract class EmeraldBurst : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected abstract string IconName { get; }

    public override string CustomPackedIconPath => EmeraldPowerIcons.Path(IconName);
    public override string CustomBigIconPath => EmeraldPowerIcons.Path(IconName);

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}

/// <summary>Ivy Burst: this turn, the debuffs you apply to enemies are multiplied by Amount (2, upgraded 3).</summary>
public sealed class EmeraldIvyBurst : EmeraldBurst
{
    protected override string IconName => "ivy";

    public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (giver != Owner || power.Type != PowerType.Debuff || target == null || target.Side == Owner.Side || amount <= 0) return 1m;
        return Amount;
    }
}

/// <summary>Slow Burst: this turn, gain Rings equal to Amount (1, upgraded 2) times the damage you deal (blocked included).</summary>
public sealed class EmeraldSlowBurst : EmeraldBurst
{
    protected override string IconName => "slow";

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == null || Owner.IsDead || result.TotalDamage <= 0 || target.Side == Owner.Side) return;
        if (dealer != Owner && dealer.PetOwner?.Creature != Owner) return;
        Flash();
        await Rings.Gain(choiceContext, Owner, result.TotalDamage * Amount);
    }
}

/// <summary>
/// Avatar Burst: the cards chosen with Avatar cost all your energy, like an X-cost card, and are played X more times
/// (X = energy spent), until they're played (not only this turn). An X-cost card stays X-cost and is also replayed X
/// times. The cost is set in a pure hook (reads the current energy); X is captured when the energy is actually spent.
/// Removed once none of its cards are waiting anymore.
/// </summary>
public sealed class EmeraldAvatarBurst : EmeraldBurst
{
    protected override string IconName => "avatar";

    private readonly List<CardModel> _chosen = [];
    private readonly Dictionary<CardModel, int> _x = [];

    public void Choose(CardModel card)
    {
        AssertMutable();
        if (!_chosen.Contains(card)) _chosen.Add(card);
        card.InvokeEnergyCostChanged();
        Flash();
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!_chosen.Contains(card) || originalCost < 0 || card.Pile?.Type != PileType.Hand) return false;
        modifiedCost = Owner.Player?.PlayerCombatState?.Energy ?? 0;
        return true;
    }

    // Real plays only (SpendResources): the card that uses it up
    public override Task AfterEnergySpent(CardModel card, int amount)
    {
        if (!_chosen.Contains(card)) return Task.CompletedTask;
        _x[card] = Math.Max(0, amount);
        Flash();
        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        _x.TryGetValue(card, out var x) && x > 0 ? playCount + x : playCount;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!_chosen.Remove(card)) return;
        _x.Remove(card);
        if (_chosen.Count == 0) await PowerCmd.Remove(this);
    }

    // Lasts until its cards are played, not just this turn
    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants) =>
        Task.CompletedTask;
}
