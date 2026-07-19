using MegaCrit.Sts2.Core.Entities.Creatures;
using Godot;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BerserkIndicator : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.None;
    protected override bool IsVisibleInternal => false;
    
    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(
        HealthBarForecastContext context)
    {
        decimal fiftyPercentHp = Owner.MaxHp * 0.5m;
        bool isBerserk = ShionMechanicsHelper.IsBerserk(Owner);
        
        // Create the segment showing the 50% threshold
        var segment = new HealthBarForecastSegment
        {
            // Position at 50% HP
            Amount = (int) fiftyPercentHp,
            
            // Visual styling
            Color = isBerserk 
                ? new Color("#eb7f2d", 1f)
                : new Color("#eb7f2d", 0f),
            
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
}
