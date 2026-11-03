using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Gloat X: if you take unblocked attack damage this turn, gain X energy and draw X additional cards next turn.
/// Lasts through the enemy turn, triggers once, then is removed. Wears off when the enemy turn ends.
/// </summary>
public class Gloat : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<EnergyNextTurnPower>(),
        HoverTipFactory.FromPower<DrawCardsNextTurnPower>(),
    ];

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.UnblockedDamage <= 0 || !props.IsPoweredAttack() || dealer is null || dealer == Owner) return;

        var amount = Amount;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner, amount, Owner, null);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner, amount, Owner, null);

        if (Owner.CombatState is null) return;
        foreach (var listener in Owner.CombatState.IterateHookListeners().OfType<ISeelsListener>().ToList())
        {
            await listener.AfterGloatTriggered(choiceContext, Owner, amount);
        }
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        return side != Owner.Side ? PowerCmd.Remove(this) : Task.CompletedTask;
    }
}
