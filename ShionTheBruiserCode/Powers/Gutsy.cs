using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Gutsy : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VigorNextTurn>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
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

        // Can trigger mid-attack, so it goes through VigorNextTurn: adding VigorPower while
        // it's tracking an attack leaves it stuck and never applied again.
        var vigorAmount = (int)(amountTaken * Amount / 100m);
        if (vigorAmount > 0)
        {
            await PowerCmd.Apply<VigorNextTurn>(
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
        if (blockedVigorAmount <= 0) return;
        await PowerCmd.Apply<VigorNextTurn>(
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