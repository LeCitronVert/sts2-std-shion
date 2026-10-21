using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Hardened Shell lasting until your next turn: you can't lose more than (Cap) HP this turn.
/// Amount is the number of turns. Cap can be 0 (which a regular Hardened Shell can't represent).
/// </summary>
public class TemporaryHardenedShell : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => Math.Max(0, Cap - _hpLostThisTurn);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<HardenedShellPower>()];

    public int Cap { get; set; }

    private int _hpLostThisTurn;

    public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0) return amount;
        return Math.Min(amount, DisplayAmount);
    }

    public override Task AfterModifyingHpLostBeforeOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0)
        {
            _hpLostThisTurn += result.UnblockedDamage;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        _hpLostThisTurn = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        return player == Owner.Player ? PowerCmd.TickDownDuration(this) : Task.CompletedTask;
    }
}
