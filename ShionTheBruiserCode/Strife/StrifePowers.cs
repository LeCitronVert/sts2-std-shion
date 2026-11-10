using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Hidden power on the Act 4 Architect. Like Obscura and Parafright, his buffs are shared with ALL the enemies he summons:
/// his Strength / Vigor buffs go on him and every living summon (<see cref="StrifeArchitect"/>'s buff move), and a summon
/// that joins later gets the Strength his buffs gave him and his current Vigor. Debuffs on him are never shared. Also:
/// <list type="bullet">
/// <item>His HP is infinite behind the scenes: a hit never takes him below 1 HP, it goes back to full after each hit, and
/// he can't die until he's "defeated".
/// The damage he takes is added up instead (<see cref="StrifeArchitect.TotalDamage"/>).</item>
/// <item>Gives each enemy that joins the fight its Ring Bounty (<see cref="StrifeConfig.BountyFor"/>).</item>
/// <item>Triggers <see cref="FieldExperiment"/> and <see cref="MakeYourPoint"/> when he takes damage.</item>
/// </list>
/// </summary>
public sealed class ArchitectsDesign : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    /// <summary>Set by the Architect only when he's "defeated".</summary>
    public bool AllowDeath { get; set; }

    /// <summary>
    /// Room type of the encounter being summoned right now (Elite / Boss), so the creatures it adds get that bounty.
    /// Null for creatures spawned mid-fight by the summons themselves.
    /// </summary>
    public RoomType? PendingSummonType { get; set; }

    private StrifeArchitect? Architect => Owner.Monster as StrifeArchitect;

    private IEnumerable<Creature> LivingSummons =>
        Owner.CombatState?.GetTeammatesOf(Owner).Where(c => c != Owner && c.IsAlive) ?? [];

    public override bool ShouldDie(Creature creature) => creature != Owner || AllowDeath;

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature == Owner || creature.Side != Owner.Side || creature.Monster == null || Owner.IsDead) return;
        var context = new ThrowingPlayerChoiceContext();

        var bounty = StrifeConfig.BountyFor(creature.Monster, PendingSummonType);
        if (bounty > 0) await PowerCmd.Apply<RingBounty>(context, creature, bounty, Owner, null);

        // Only what his own buffs gave him: debuffs on him (Make Your Point, players' Strength down...) aren't shared
        var strength = Architect?.SharedStrength ?? 0;
        if (strength > 0) await PowerCmd.Apply<StrengthPower>(context, creature, strength, Owner, null);
        var vigor = Owner.GetPowerAmount<VigorPower>();
        if (vigor > 0) await PowerCmd.Apply<VigorPower>(context, creature, vigor, Owner, null);
    }

    /// <summary>Damage of the current hit before it was capped (see <see cref="ModifyHpLostAfterOstyLate"/>).</summary>
    private decimal _uncappedHpLoss;

    // Only called by real damage. The hit never takes him below 1 HP: at 0 his node plays its death and is removed,
    // even though the death itself is prevented. The full amount still counts (score, Field Experiment, Make Your Point).
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || AllowDeath || amount <= 0) return amount;
        _uncappedHpLoss += amount;
        return Math.Min(amount, Math.Max(0, Owner.CurrentHp - 1));
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || AllowDeath) return;

        var unblocked = (int)Math.Max(result.UnblockedDamage, _uncappedHpLoss);
        _uncappedHpLoss = 0;
        var total = result.BlockedDamage + unblocked;
        Architect?.AddDamage(total);
        // Infinite HP behind the scenes
        if (Owner.CurrentHp < Owner.MaxHp) Owner.SetCurrentHpInternal(Owner.MaxHp);

        if (total > 0 && Owner.GetPower<FieldExperiment>() is { } fieldExperiment)
        {
            await fieldExperiment.Spread(choiceContext, total, LivingSummons.ToList());
        }

        if (unblocked > 0 && Owner.GetPower<MakeYourPoint>() is { } makeYourPoint)
        {
            await makeYourPoint.Reduce(choiceContext, unblocked);
        }
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Owner && creature.Side == Owner.Side && Owner.IsAlive)
        {
            Architect?.OnSummonDied();
            // Visual only: he stands back up once his last summon is gone, and the others close the gap
            StrifeLayout.ArrangeDeferred();
        }
        return Task.CompletedTask;
    }
}

/// <summary>All damage applied to the Architect is spread evenly between the other enemies, if any are alive.</summary>
public sealed class FieldExperiment : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public async Task Spread(PlayerChoiceContext choiceContext, int damage, IReadOnlyList<Creature> summons)
    {
        if (summons.Count == 0 || damage <= 0) return;
        Flash();

        var share = damage / summons.Count;
        var remainder = damage % summons.Count;
        // The remainder goes to random summons, one point each
        var extra = summons.ToList();
        Owner.CombatState!.RunState.Rng.CombatTargets.Shuffle(extra);
        var bonus = extra.Take(remainder).ToHashSet();

        foreach (var summon in summons)
        {
            var amount = share + (bonus.Contains(summon) ? 1 : 0);
            if (amount <= 0 || summon.IsDead) continue;
            await CreatureCmd.Damage(choiceContext, summon, amount, ValueProp.Unpowered, (Creature?)null, null);
        }
    }
}

/// <summary>
/// Dealing unblocked damage to the Architect reduces his Strength by that much until the end of his turn.
/// Always shown; its number is the Strength currently taken away.
/// </summary>
public sealed class MakeYourPoint : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _reduction;

    public override int DisplayAmount => _reduction;

    public async Task Reduce(PlayerChoiceContext choiceContext, int amount)
    {
        if (amount <= 0) return;
        Flash();
        _reduction += amount;
        InvokeDisplayAmountChanged();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -amount, Owner, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || _reduction <= 0) return;
        var restored = _reduction;
        _reduction = 0;
        InvokeDisplayAmountChanged();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, restored, Owner, null);
    }
}

/// <summary>
/// After playing more than (Limit) cards in a single turn, your turn ends. Each player is counted separately.
/// Its number is the Limit; the Architect lowers it during his buff cycle.
/// </summary>
public sealed class GrowingImpatience : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Created on first use: a field initializer would be shared with the canonical model by MutableClone
    private Dictionary<Player, int>? _playedThisTurn;

    /// <summary>
    /// Cards that player can still play this turn: the (Limit + 1)th card ends it, so it still counts.
    /// Read-only (for the indicator next to the energy counter).
    /// </summary>
    public int CardsLeft(Player player) => Math.Max(0, Amount + 1 - (_playedThisTurn?.GetValueOrDefault(player) ?? 0));

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        if (player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding) return;
        if (Owner.CombatState?.CurrentSide != CombatSide.Player) return;

        _playedThisTurn ??= new Dictionary<Player, int>();
        var played = _playedThisTurn.GetValueOrDefault(player) + 1;
        _playedThisTurn[player] = played;
        // "After playing more than (Limit) cards"
        if (played <= Amount) return;

        Flash();
        // Like Void Form: every client ends that player's turn at the same point
        PlayerCmd.EndTurn(player, canBackOut: false);
        await Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player) _playedThisTurn?.Clear();
        return Task.CompletedTask;
    }
}

/// <summary>
/// When this enemy dies, every player gains this many Rings (Gold). At the end of each of its turns, it drops by the
/// number of turns it has been alive (1, then 2, then 3...).
/// </summary>
public sealed class RingBounty : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "ring_bounty.png".PowerImagePath();
    public override string CustomBigIconPath => "ring_bounty.png".BigPowerImagePath();

    private int _turnsAlive;

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Owner || Amount <= 0 || Owner.CombatState is not { } combatState) return;
        Flash();
        foreach (var player in combatState.Players)
        {
            await PlayerCmd.GainGold(Amount, player);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.IsDead) return;
        _turnsAlive++;
        await PowerCmd.ModifyAmount(choiceContext, this, -Math.Min(Amount, _turnsAlive), null, null, silent: true);
    }
}
