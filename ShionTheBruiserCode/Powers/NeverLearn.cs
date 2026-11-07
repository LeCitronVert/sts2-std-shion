using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you gain energy from Gloat, gain as much Strength (times the stacks).
/// The Strength comes with the energy, at the start of your next turn.
/// </summary>
public class NeverLearn : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Gloat>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    /// <summary>Energy Gloat will give at the start of the next turn.</summary>
    private int _pendingGloatEnergy;

    public Task AfterGloatTriggered(PlayerChoiceContext choiceContext, Creature creature, int energy)
    {
        if (creature == Owner && energy > 0) _pendingGloatEnergy += energy;
        return Task.CompletedTask;
    }

    // Same timing as EnergyNextTurnPower, which gives Gloat's energy.
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || _pendingGloatEnergy <= 0) return;

        var strength = _pendingGloatEnergy * Amount;
        _pendingGloatEnergy = 0;
        Flash();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, strength, Owner, null);
    }
}
