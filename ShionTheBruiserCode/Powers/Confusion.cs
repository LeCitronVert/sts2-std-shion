using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;
  
public class Confusion(): SlalTheEndlessPower
{
  public override PowerType Type => PowerType.Debuff;
  public override PowerStackType StackType => PowerStackType.Counter;
  public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;

  public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
  {
    var power = this;
    if (side != CombatSide.Player)
    {
      return;
    }
    var canBeLethal = Owner.CurrentHp == 1;
    var damageAmount = Amount;
    if (!canBeLethal)
    {
      damageAmount = Math.Min(Amount, Owner.CurrentHp - 1);
    }
    
    await CreatureCmd.Damage(
      new ThrowingPlayerChoiceContext(), 
      power.Owner, 
      damageAmount, 
      ValueProp.Unblockable | ValueProp.Unpowered, 
      null, 
      null
      );
    await PowerCmd.Remove(this);
  }
  
  public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(
    HealthBarForecastContext context)
  {
    var fragmentAmount = Amount;
    var canBeLethal = Owner.CurrentHp == 1;
    if (!canBeLethal) fragmentAmount = Math.Min(Amount, Owner.CurrentHp - 1);
        
    var segment = new HealthBarForecastSegment
    {
      Amount = fragmentAmount,
      Color = new Color("#40E0D0", 1f),
      Direction = HealthBarForecastDirection.FromRight,
      Order = 0,
    };
        
    yield return segment;
  }
}