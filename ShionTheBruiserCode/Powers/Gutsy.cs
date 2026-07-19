using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Gutsy : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? _,
        CardModel? __)
    {
        var power = this;
        var amountTaken = result.UnblockedDamage;
        if (target != power.Owner || !props.IsPoweredAttack())
            return;

        if (amountTaken > 0m)
        {
            var vigorAmount = (int)(amountTaken * Amount / 100m);
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                Owner,
                vigorAmount,
                Owner,
                null
            );
        }
        
        if (!ShionMechanicsHelper.IsBerserk(Owner))
        {
            return;
        }
        
        var blockedVigorAmount = (int) (result.BlockedDamage * Amount / 2m / 100m);
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            Owner,
            blockedVigorAmount,
            Owner,
            null
        );
    }
    
    // Berserk Power stuff
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner) return Task.CompletedTask;
        
        ChangePulse();
        return Task.CompletedTask;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ChangePulse();
        
        return Task.CompletedTask;
    }
    

    public void ChangePulse()
    {
        var shouldPulse = ShionMechanicsHelper.IsBerserk(Owner);

        if (shouldPulse)
        {
            StartPulsing();
        }
        else
        {
            StopPulsing();
        }
    }
}