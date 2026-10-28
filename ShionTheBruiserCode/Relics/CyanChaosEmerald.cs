using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever playing a Power card gives you a permanent buff you didn't have yet this combat, gain 1 Plating.
/// Plating itself doesn't count. "Permanent" can't be read from the power, so temporary buffs are listed in <see cref="IsTemporaryBuff"/>.
/// </summary>
public class CyanChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

    /// <summary>
    /// Buffs that expire on their own (this turn, next turn, until the next attack).
    /// </summary>
    private static readonly HashSet<Type> TemporaryBuffs = [
        typeof(VigorPower),
        typeof(BurlesqueWindUp),
        typeof(BurstOfLaughter),
        typeof(KickstartEvent),
        typeof(MemoryThisTurn),
        typeof(OfferLife),
        typeof(RandomBlast),
        typeof(RevengeDrive),
        typeof(SlimedFreeThisTurn),
    ];

    public static bool IsTemporaryBuff(PowerModel power)
    {
        var type = power.GetType();
        if (TemporaryBuffs.Contains(type)) return true;
        if (type.Name.Contains("NextTurn") || type.Name.Contains("ThisTurn")) return true;

        // Temporary Strength/Dexterity/Focus and BaseLib's temporary wrappers
        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.Name.StartsWith("Temporary") || t.Name.StartsWith("CustomTemporary")) return true;
        }
        return false;
    }

    private readonly HashSet<ModelId> _buffsGainedThisCombat = [];

    public override Task BeforeCombatStart()
    {
        _buffsGainedThisCombat.Clear();
        foreach (var power in Owner.Creature.Powers.Where(p => p.Type == PowerType.Buff))
        {
            _buffsGainedThisCombat.Add(power.Id);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner != Owner.Creature || amount <= 0 || power.Type != PowerType.Buff) return;
        if (power is PlatingPower || IsTemporaryBuff(power)) return;
        if (!_buffsGainedThisCombat.Add(power.Id)) return;
        if (cardSource is not { Type: CardType.Power } || cardSource.Owner != Owner) return;

        Flash();
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, null);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _buffsGainedThisCombat.Clear();
        return Task.CompletedTask;
    }
}
