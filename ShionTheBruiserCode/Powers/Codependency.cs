using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Codependency : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public const int MaxDamage = 50;
    
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
            
            var missingHpRatio = (partner.MaxHp - partner.CurrentHp) / (float)partner.MaxHp;
            var currentPercentage = (int)Math.Round(missingHpRatio * (float)Amount);
            return currentPercentage;
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
        
        var missingHpRatio = (partner.MaxHp - partner.CurrentHp) / (float)partner.MaxHp;

        return (decimal)(missingHpRatio * (float)(Amount * 0.01M)) + 1;
    }
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ((StringVar) this.DynamicVars["Partner"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, this.Applier!.Player.NetId);
        InvokeDisplayAmountChanged();
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        
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
        }
        return Task.CompletedTask;
    }
}