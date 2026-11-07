using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of your turn, gain 2 energy (per stack).
/// Increase the maximum energy cost an Erratic card can have by X.
/// </summary>
public class BigNumbers : SeelsTheRecklessPower
{
    public const int EnergyPerStack = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Shows the maximum cost an Erratic card can reach, not the stacks.</summary>
    public override int DisplayAmount => SeelsMechanicsHelper.BaseErraticMaxCost + Amount;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic)];

    // After the energy reset, like the base game's turn-start energy, so it isn't wiped.
    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player) return;

        Flash();
        await PlayerCmd.GainEnergy(EnergyPerStack * Amount, player);
    }
}
