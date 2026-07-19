using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class PleasureInPain() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return target != this.Owner ? 1M : 1 + (Amount / 100M);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        return side == CombatSide.Enemy ? PowerCmd.Remove(this) : Task.CompletedTask;
    }
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? _,
        CardModel? __)
    {
        var power = this;
        if (target != power.Owner || !props.IsPoweredAttack())
            return;
        
        var strengthAmount = (int) (result.BlockedDamage * Amount / 100m);
        var vigorAmount = (int) (result.UnblockedDamage * Amount / 100m);
        var patchUpAmount = (int) (result.TotalDamage * Amount / 100m);
        
        if (strengthAmount > 0) {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                Owner,
                strengthAmount,
                Owner,
                null
            );
        }

        if (vigorAmount > 0)
        {
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                Owner,
                vigorAmount,
                Owner,
                null
            );
        }
        
        if (ShionMechanicsHelper.IsBerserk(Owner) && patchUpAmount > 0) {
            await ShionMechanicsHelper.PatchUpCreature(patchUpAmount, Owner);
        }
    }
}