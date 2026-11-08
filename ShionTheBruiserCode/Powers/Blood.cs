using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Seels' resource, consumed by Blood Bend. Gained by losing HP (Thorn Bracelet) or dealing damage with Maim cards.
/// Capped by missing HP: Blood gained above it is lost, and healing removes the Blood above it.
/// Not shown as a power icon: it's drawn on the health bar (<see cref="Visuals.BloodHealthBarPatch"/>) and as wisps
/// around the creature (CreatureFxController).
/// </summary>
public class Blood : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta <= 0) return;
        // No choice context in this hook; trimming Blood never asks the player anything.
        await SeelsMechanicsHelper.TrimBloodToCap(new ThrowingPlayerChoiceContext(), Owner);
    }
}
