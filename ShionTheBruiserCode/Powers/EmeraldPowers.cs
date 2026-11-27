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

/// <summary>Water passive (Cyan): the first Power you play from your Hand each fight is played an additional time.</summary>
public sealed class EmeraldWater : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;
    protected override string IconName => "water";

    private bool Applies(CardModel card) =>
        card.Owner.Creature == Owner && card.Type == CardType.Power && !IsEmeraldPower(card) &&
        !CardAutoPlayTracker.IsBeingAutoPlayed(card);

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        Applies(card) ? playCount + 1 : playCount;

    // Once per fight: used up
    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card.Owner.Creature != Owner) return;
        Flash();
        await PowerCmd.Remove(this);
    }
}

/// <summary>Vision passive (Purple): whenever you're attacked, draw 1 additional card at the start of your turn (per attack).</summary>
public sealed class EmeraldVision : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Purple;
    protected override string IconName => "vision";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DrawCardsNextTurnPower>()];

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (Owner.IsDead || command.Attacker is not { } attacker || attacker.Side == Owner.Side) return;
        if (!command.Results.SelectMany(r => r).Any(r => r.Receiver == Owner)) return;
        Flash();
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner, 1m, Owner, null);
    }
}

/// <summary>
/// Ivy passive (Green): when you deal blocked Attack damage, apply 1 Weak; when you deal unblocked Attack damage, apply
/// 1 Poison (checked per hit, both can happen on the same hit).
/// </summary>
public sealed class EmeraldIvy : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Green;
    protected override string IconName => "ivy";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<PoisonPower>()];

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || Owner.IsDead || !props.IsPoweredAttack() || target.Side == Owner.Side || !target.IsAlive) return;
        if (result.BlockedDamage > 0)
        {
            Flash();
            await PowerCmd.Apply<WeakPower>(choiceContext, target, 1m, Owner, null);
        }
        if (result.UnblockedDamage > 0 && target.IsAlive)
        {
            Flash();
            await PowerCmd.Apply<PoisonPower>(choiceContext, target, 1m, Owner, null);
        }
    }
}

/// <summary>
/// Slow passive (Yellow): whenever you gain energy during your turn, draw 1 card (per gain).
/// "Gain energy" = PlayerCmd.GainEnergy (cards, Energy Next Turn...), not the turn's energy reset
/// (<see cref="EmeraldSlowEnergyPatch"/>).
/// </summary>
public sealed class EmeraldSlow : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;
    protected override string IconName => "slow";

    public async Task AfterEnergyGained(Player player)
    {
        if (Owner.IsDead || Owner.CombatState?.CurrentSide != Owner.Side || CombatManager.Instance.IsOverOrEnding) return;
        Flash();
        await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 1m, player);
    }
}

/// <summary>Calls <see cref="EmeraldSlow"/> after each energy gain that actually gave energy.</summary>
[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainEnergy))]
public static class EmeraldSlowEnergyPatch
{
    [HarmonyPrefix]
    public static void Prefix(Player player, out int __state) => __state = player.PlayerCombatState?.Energy ?? 0;

    [HarmonyPostfix]
    public static void Postfix(Player player, int __state, ref Task __result)
    {
        if (player.Creature.GetPower<EmeraldSlow>() is null) return;
        __result = After(__result, player, __state);
    }

    private static async Task After(Task original, Player player, int before)
    {
        await original;
        var gained = (player.PlayerCombatState?.Energy ?? 0) - before;
        if (gained > 0 && player.Creature.GetPower<EmeraldSlow>() is { } slow) await slow.AfterEnergyGained(player);
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
/// Extra passive (Grey): the first card you play from your Hand each turn is played an additional time (Emerald Powers
/// are skipped and don't use it up).
/// </summary>
public sealed class EmeraldExtra : EmeraldPassive
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;
    protected override string IconName => "extra";

    private bool _usedThisTurn;

    private bool Applies(CardModel card) =>
        !_usedThisTurn && card.Owner.Creature == Owner && !IsEmeraldPower(card) && !CardAutoPlayTracker.IsBeingAutoPlayed(card);

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        Applies(card) ? playCount + 1 : playCount;

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card.Owner.Creature != Owner) return Task.CompletedTask;
        _usedThisTurn = true;
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner)) _usedThisTurn = false;
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

/// <summary>Slow Burst: for Amount turns (this one included), gain as many Rings as the unblocked damage you deal.</summary>
public sealed class EmeraldSlowBurst : EmeraldBurst
{
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override string IconName => "slow";

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == null || Owner.IsDead || result.UnblockedDamage <= 0 || target.Side == Owner.Side) return;
        if (dealer != Owner && dealer.PetOwner?.Creature != Owner) return;
        Flash();
        await Rings.Gain(choiceContext, Owner, result.UnblockedDamage);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Decrement(this);
    }
}

/// <summary>
/// Avatar Burst: the next card you play from your Hand (not an Emerald Power) costs all your energy, like an X-cost
/// card, and is played X more times (X = energy spent). An X-cost card stays X-cost and is also replayed X times.
/// The cost is set in a pure hook (reads the current energy); X is captured when the energy is actually spent.
/// </summary>
public sealed class EmeraldAvatarBurst : EmeraldBurst
{
    protected override string IconName => "avatar";

    private CardModel? _card;
    private int _x;

    private bool Eligible(CardModel card) => card.Owner.Creature == Owner && !IsEmerald(card);

    private static bool IsEmerald(CardModel card) => card is EmeraldPowerCard;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (_card != null || !Eligible(card) || originalCost < 0 || card.Pile?.Type != PileType.Hand) return false;
        modifiedCost = Owner.Player?.PlayerCombatState?.Energy ?? 0;
        return true;
    }

    // Real plays only (SpendResources): the card that uses it up
    public override Task AfterEnergySpent(CardModel card, int amount)
    {
        if (_card != null || !Eligible(card)) return Task.CompletedTask;
        _card = card;
        _x = Math.Max(0, amount);
        Flash();
        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        card == _card && _x > 0 ? playCount + _x : playCount;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != _card) return;
        await PowerCmd.Remove(this);
    }
}

/// <summary>Bullet Burst: this turn, your Attacks deal Amount% more damage for each different debuff the target has.</summary>
public sealed class EmeraldBulletBurst : EmeraldBurst
{
    protected override string IconName => "bullet";

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        if (dealer != Owner || target == null || target.Side == Owner.Side || !props.IsPoweredAttack()) return 1m;
        var debuffs = target.Powers.Where(p => p.Type == PowerType.Debuff && p.IsVisible).Select(p => p.GetType()).Distinct().Count();
        return 1m + Amount / 100m * debuffs;
    }
}
