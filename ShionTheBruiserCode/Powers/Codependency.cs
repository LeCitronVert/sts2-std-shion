using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Codependency : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public const int BerserkMultiplier = 2;
    
    /// <summary>
    /// Display the current damage bonus percentage based on partner's missing HP.
    /// </summary>
    public override int DisplayAmount
    {
        get
        {
            var partner = Applier;
            if (partner == null)
            {
                return 0;
            }
            
            return ShionMechanicsHelper.IsBerserk(partner) ? BerserkMultiplier * Amount : Amount;
        }
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("Partner"),
        new DynamicVar("MaxDamage", Amount),
        new DynamicVar("DisplayAmount", 0)
    ];
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return dealer != this.Owner || !props.IsPoweredAttack()  ? 1M : GetDamageBoost();
    }
    
    public decimal GetDamageBoost()
    {
        var partner = Applier;
        if (partner == null)
        {
            return 1M;
        }
        
        return (decimal) (float) (Amount * 0.01M * (ShionMechanicsHelper.IsBerserk(partner) ? 2 : 1)) + 1;
    }
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ((StringVar) this.DynamicVars["Partner"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, this.Applier!.Player!.NetId);
        InvokeDisplayAmountChanged();
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        ChangePulse();
        
        return Task.CompletedTask;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Applier) return;
        
        await PowerCmd.Remove(this);
        await CreatureCmd.Kill(Owner);
    }

    /// <summary>
    /// Update the display when any creature's HP changes (owner or partner).
    /// </summary>
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // Update display if either the owner or the partner's HP changes
        if (creature == Owner || creature == Applier)
        {
            InvokeDisplayAmountChanged();
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
            ChangePulse();
        }
        return Task.CompletedTask;
    }
    
    // Berserk Power stuff
    public void ChangePulse()
    {
        var partner = Applier;
        if (partner == null) return;
        var shouldPulse = ShionMechanicsHelper.IsBerserk(partner);

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