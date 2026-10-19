using MegaCrit.Sts2.Core.Entities.Creatures;
using Godot;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BerserkIndicator : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.None;
    protected override bool IsVisibleInternal => false;
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(
        HealthBarForecastContext context)
    {
        // Fell Dragon makes you always Berserk, so the indicator covers the whole bar
        decimal thresholdHp = Owner.HasPower<FellDragon>()
            ? Owner.MaxHp
            : Owner.MaxHp * ShionMechanicsHelper.GetBerserkThreshold(Owner);
        bool isBerserk = ShionMechanicsHelper.IsBerserk(Owner);

        // Create the segment showing the Berserk threshold (50% by default)
        var segment = new HealthBarForecastSegment
        {
            // Position at the Berserk threshold
            Amount = (int) thresholdHp,
            
            // Visual styling
            Color = isBerserk 
                ? new Color("#e07870", 1f)
                : new Color("#e07870", 0f),
            
            Direction = HealthBarForecastDirection.FromLeft,
            
            Order = 1,
        };
        
        yield return segment;
    }
    
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner)
        {
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
    
    // Pleasure In Pain, Endure and Fell Dragon change the threshold / Berserk state
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner == Owner && power != this)
        {
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
