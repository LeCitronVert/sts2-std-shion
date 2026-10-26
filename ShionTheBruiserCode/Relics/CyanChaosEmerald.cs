using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever playing a Power card gives you a buff you didn't have yet this combat, gain 1 Max HP this combat only.
/// </summary>
public class CyanChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(1)];

    private readonly HashSet<ModelId> _buffsGainedThisCombat = [];
    private decimal _maxHpGainedThisCombat;

    public override Task BeforeCombatStart()
    {
        _buffsGainedThisCombat.Clear();
        _maxHpGainedThisCombat = 0;
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
        if (!_buffsGainedThisCombat.Add(power.Id)) return;
        if (cardSource is not { Type: CardType.Power } || cardSource.Owner != Owner) return;

        Flash();
        var gain = DynamicVars.MaxHp.BaseValue;
        await CreatureCmd.GainMaxHp(Owner.Creature, gain);
        _maxHpGainedThisCombat += gain;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (_maxHpGainedThisCombat > 0)
        {
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature, _maxHpGainedThisCombat, false);
        }
        _maxHpGainedThisCombat = 0;
        _buffsGainedThisCombat.Clear();
    }
}
