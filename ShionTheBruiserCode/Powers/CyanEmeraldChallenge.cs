using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Cyan challenge (on each player): at the end of your turn, lose HP equal to the turn number (x stacks).
/// </summary>
public class CyanEmeraldChallenge : EmeraldChallengePower
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;

    /// <summary>
    /// Display the HP that will be lost at the end of this turn.
    /// </summary>
    public override int DisplayAmount => (Owner.Player?.PlayerCombatState?.TurnNumber ?? 1) * Amount;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power == this) InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.Player?.PlayerCombatState is not { } combatState) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, combatState.TurnNumber * Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }
}
