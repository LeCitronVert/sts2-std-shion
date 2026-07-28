using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiser.images.powers;

public class CompetitiveSpirit() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public int AmountThisTurn = 0;
    public bool Initialized;
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        AmountThisTurn = Amount;
        Initialized = true;
    }
    
    public void AdaptAmountAsNeeded(int amount)
    {
        if (Initialized) AmountThisTurn += amount;
        else Initialize();
    }
    
    public async Task GainStrengthIfOtherBuff()
    {
        if (AmountThisTurn <= 0) return;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null!);
        if (ShionMechanicsHelper.IsBerserk(Owner))
        {
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null!);
        }
        AmountThisTurn--;
        this.Flash();
    }
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (Owner == power.Owner && power is CompetitiveSpirit)
        {
            AdaptAmountAsNeeded((int)amount);
        }
        
        if (Owner != power.Owner && power.Type == PowerType.Buff)
        {
            await GainStrengthIfOtherBuff();
        }
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