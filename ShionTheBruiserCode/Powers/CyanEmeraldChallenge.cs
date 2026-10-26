using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Cyan challenge (on each player): at the end of your turn, lose HP equal to the turn number (x stacks).
/// </summary>
public class CyanEmeraldChallenge : EmeraldChallengePower
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.Player?.PlayerCombatState is not { } combatState) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, combatState.TurnNumber * Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }
}
