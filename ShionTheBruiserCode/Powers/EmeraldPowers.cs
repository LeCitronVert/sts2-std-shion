using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>The Emerald wheel's section icons (images/ui/emerald_wheel), reused as the Emerald powers' icons.</summary>
internal static class EmeraldPowerIcons
{
    public static string Path(string name) =>
        System.IO.Path.Join(MainFile.ResPath, "images", "ui", "emerald_wheel", name + "_available.png");
}

/// <summary>
/// The lasting effect of an Emerald Power card. Only one can be active: playing an Emerald Power first removes the
/// active one (<see cref="Cards.EmeraldPowerCard"/>).
/// </summary>
public abstract class EmeraldPowerEffect : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected abstract string IconName { get; }

    // Same icon as its Emerald wheel section
    public override string CustomPackedIconPath => EmeraldPowerIcons.Path(IconName);
    public override string CustomBigIconPath => EmeraldPowerIcons.Path(IconName);

    public static async Task RemoveAll(Creature creature)
    {
        foreach (var effect in creature.Powers.OfType<EmeraldPowerEffect>().ToList())
        {
            await PowerCmd.Remove(effect);
        }
    }
}

/// <summary>Water (Cyan Chaos Emerald): whenever you play a Power from your Hand, play a copy of that Power.</summary>
public sealed class EmeraldWater : EmeraldPowerEffect
{
    protected override string IconName => "water";

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner.Creature != Owner || card.Type != CardType.Power || cardPlay.IsAutoPlay || !cardPlay.IsFirstInSeries) return;
        if (Owner.CombatState is not { } combatState || CombatManager.Instance.IsOverOrEnding || Owner.IsDead) return;

        Flash();
        var copy = combatState.CloneCard(card);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, card.Owner);
        await CardCmd.AutoPlay(choiceContext, copy, null);
    }
}

/// <summary>Vision (Purple Chaos Emerald): draw (Amount) additional cards at the start of your turn.</summary>
public sealed class EmeraldVision : EmeraldPowerEffect
{
    protected override string IconName => "vision";

    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player.Creature == Owner ? count + Amount : count;
}

/// <summary>
/// Ivy (Green Chaos Emerald): whenever you play a Skill, apply (Amount) Poison to ALL enemies. Your Attacks apply
/// (Weak) Weak (upgraded card only).
/// </summary>
public sealed class EmeraldIvy : EmeraldPowerEffect
{
    public const string WeakKey = "Weak";

    protected override string IconName => "ivy";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(WeakKey, 0)];

    public void SetWeak(decimal amount)
    {
        AssertMutable();
        DynamicVars[WeakKey].BaseValue = amount;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Skill || Owner.CombatState is not { } combatState) return;
        var enemies = combatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        Flash();
        await PowerCmd.Apply<PoisonPower>(choiceContext, enemies, Amount, Owner, null);
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var weak = DynamicVars[WeakKey].BaseValue;
        if (weak <= 0 || command.Attacker != Owner || command.ModelSource is not CardModel) return;
        var targets = command.Results.SelectMany(r => r).Select(r => r.Receiver)
            .Distinct().Where(c => c.IsAlive && c.Side != Owner.Side).ToList();
        if (targets.Count == 0) return;
        Flash();
        await PowerCmd.Apply<WeakPower>(choiceContext, targets, weak, Owner, null);
    }
}

/// <summary>
/// Slow (Yellow Chaos Emerald): whenever you gain energy, gain as many Rings (times the stacks).
/// "Gain energy" = PlayerCmd.GainEnergy (cards, Energy Next Turn...), not the turn's energy reset
/// (<see cref="EmeraldSlowEnergyPatch"/>).
/// </summary>
public sealed class EmeraldSlow : EmeraldPowerEffect
{
    protected override string IconName => "slow";

    public async Task AfterEnergyGained(int amount)
    {
        if (amount <= 0 || Owner.IsDead) return;
        Flash();
        await Rings.Gain(new ThrowingPlayerChoiceContext(), Owner, amount * Amount);
    }
}

/// <summary>Calls <see cref="EmeraldSlow"/> after each energy gain, with the energy actually gained.</summary>
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
        if (gained > 0 && player.Creature.GetPower<EmeraldSlow>() is { } slow) await slow.AfterEnergyGained(gained);
    }
}

/// <summary>Avatar (Blue Chaos Emerald): at the end of your turn, gain 1 energy next turn for each card in your Hand.</summary>
public sealed class EmeraldAvatar : EmeraldPowerEffect
{
    protected override string IconName => "avatar";

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        var cards = PileType.Hand.GetPile(player).Cards.Count;
        if (cards <= 0) return;
        Flash();
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, cards * Amount, Owner, null);
    }
}

/// <summary>Bullet (Red Chaos Emerald): whenever you attack a Vulnerable enemy, heal 1 (per enemy hit).</summary>
public sealed class EmeraldBullet : EmeraldPowerEffect
{
    protected override string IconName => "bullet";

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner || Owner.IsDead) return;
        var vulnerable = command.Results.SelectMany(r => r).Select(r => r.Receiver)
            .Distinct().Count(c => c.Side != Owner.Side && c.HasPower<VulnerablePower>());
        if (vulnerable <= 0) return;
        Flash();
        await CreatureCmd.Heal(Owner, vulnerable * Amount);
    }
}

/// <summary>
/// Extra (Grey Chaos Emerald): the first (Amount) Attacks you play each turn gain Replay 1 after they are played (so they
/// need to be drawn again to benefit from it).
/// </summary>
public sealed class EmeraldExtra : EmeraldPowerEffect
{
    protected override string IconName => "extra";

    public override int DisplayAmount => Math.Max(0, Amount - _attacksThisTurn);

    private int _attacksThisTurn;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner.Creature != Owner || card.Type != CardType.Attack || !cardPlay.IsFirstInSeries) return Task.CompletedTask;
        if (_attacksThisTurn >= Amount) return Task.CompletedTask;

        _attacksThisTurn++;
        card.BaseReplayCount += 1;
        Flash();
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            _attacksThisTurn = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
