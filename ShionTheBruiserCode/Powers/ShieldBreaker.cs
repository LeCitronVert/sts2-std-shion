using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class ShieldBreaker : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return card.Owner.Creature == this.Owner && card.Type == CardType.Attack && target?.Block > 0 ? playCount + Amount : playCount;
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner && dealer?.PetOwner != Owner.Player || target.IsPlayer || (!result.WasBlockBroken && target.Block <= 0) || !ShionMechanicsHelper.IsBerserk(Owner))
            return;
        Flash();
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, 
            target, 
            Amount, 
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


