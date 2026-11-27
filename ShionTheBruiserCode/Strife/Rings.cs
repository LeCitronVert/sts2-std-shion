using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Rings: a buff of their own (no longer the player's Gold). Unblocked damage you take is absorbed by your Rings first,
/// 1 Ring per point, until you run out. Damage you deal yourself isn't absorbed.
/// With <see cref="SuperFormPower"/>, that protection can be turned off and back on from the Emerald wheel; without it,
/// it's always on.
/// Rings only last for the fight: whatever is left when a fight is won becomes Gold (<see cref="RingsToGoldSingleton"/>).
/// Gained from Emerald Powers, Yellow Emerald (Slow), the Architect fight and the Chaos Emeralds' Act 4 shrine.
/// Shown in the top bar (<see cref="RingTopBar"/>) and on the health bar (<see cref="RingHealthBarPatch"/>).
/// </summary>
public sealed class Rings : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // The old top bar Ring icon (when Rings were Gold)
    public override string CustomPackedIconPath => RingVisuals.RingIconPath;
    public override string CustomBigIconPath => RingVisuals.RingIconPath;

    private int _pendingLoss;

    public static int Get(Creature? creature) => creature?.GetPowerAmount<Rings>() ?? 0;

    /// <summary>Whether this creature's Rings absorb damage right now (logic and visuals).</summary>
    public static bool IsProtecting(Creature creature) =>
        creature.IsAlive && Get(creature) > 0 && creature.GetPower<SuperFormPower>() is not { ProtectionOn: false };

    public static async Task Gain(PlayerChoiceContext choiceContext, Creature creature, decimal amount)
    {
        if (amount <= 0 || creature.IsDead || creature.CombatState is null) return;
        await PowerCmd.Apply<Rings>(choiceContext, creature, amount, creature, null);
    }

    // Only called by real damage (not by previews), so tracking the absorbed amount here is safe
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0 || dealer == Owner || !IsProtecting(Owner)) return amount;
        var available = Amount - _pendingLoss;
        if (available <= 0) return amount;

        var absorbed = (int)Math.Min(available, Math.Ceiling(amount));
        _pendingLoss += absorbed;
        return Math.Max(0m, amount - absorbed);
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_pendingLoss <= 0) return;
        var loss = _pendingLoss;
        _pendingLoss = 0;
        Flash();
        if (loss >= Amount) await PowerCmd.Remove(this);
        else await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -loss, null, null);
    }
}

/// <summary>
/// When a fight is won, each player's leftover Rings become Gold.
/// AfterCombatEnd (victory only) runs before Player.AfterCombatEnd() removes the powers (see BloodCarryOverSingleton).
/// </summary>
public class RingsToGoldSingleton() : CustomSingletonModel(HookType.Combat)
{
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        foreach (var player in room.CombatState.Players)
        {
            var rings = Rings.Get(player.Creature);
            if (rings > 0 && player.Creature.IsAlive) await PlayerCmd.GainGold(rings, player);
        }
    }
}
