using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you Shapeshift or Mimic, apply X Confusion to ALL enemies, and gain the created card's cost:
/// the energy and/or stars needed to play it at its unmodified cost (upgrades count, but not "free this turn",
/// so Free Shapeshift still refunds the Goofball Token Power's cost). X costs give nothing.
/// Called by <see cref="SlalMechanicsHelper"/> after each Shapeshift / Mimic.
/// </summary>
public class PinwheelForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
    ];

    /// <param name="created">The card the Shapeshift or Mimic added (null if the Mimic was skipped).</param>
    public async Task AfterShapeshiftOrMimic(PlayerChoiceContext choiceContext, CardModel? created)
    {
        if (CombatState is null || Owner.Player is not { } player) return;

        Flash();
        // No card source: Confusion from powers isn't increased by Random
        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, Owner, Amount, null);

        if (created is null) return;

        var energy = created.EnergyCost.CostsX ? 0 : created.EnergyCost.GetWithModifiers(CostModifiers.None);
        if (energy > 0)
        {
            await PlayerCmd.GainEnergy(energy, player);
        }

        var stars = created.HasStarCostX ? 0 : created.BaseStarCost;
        if (stars > 0)
        {
            await PlayerCmd.GainStars(stars, player);
        }
    }
}
