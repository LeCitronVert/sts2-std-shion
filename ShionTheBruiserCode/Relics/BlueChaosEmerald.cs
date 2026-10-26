using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you gain energy from a card, gain 1 extra energy.
/// "From a card" = while a card play is resolving (between its CardPlayStarted and CardPlayFinished history entries),
/// so powers reacting after the play (AfterCardPlayed) don't count.
/// </summary>
public class BlueChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Blue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        if (player != Owner || amount <= 0 || !IsCardPlayResolving()) return amount;
        return amount + DynamicVars.Energy.BaseValue;
    }

    public override Task AfterModifyingEnergyGain()
    {
        Flash();
        return Task.CompletedTask;
    }

    private static bool IsCardPlayResolving()
    {
        if (!CombatManager.Instance.IsInProgress) return false;
        var history = CombatManager.Instance.History;
        return history.CardPlaysStarted.Count() > history.CardPlaysFinished.Count();
    }
}
