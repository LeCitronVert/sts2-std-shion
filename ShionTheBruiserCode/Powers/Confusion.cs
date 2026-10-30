using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of its turn, loses X HP and loses all Confusion. Does not drop below 1 HP.
/// Attacks execute it if they leave it with as much Confusion as HP or more
/// (even if the attack dealt no damage). Executed enemies spin off the screen (see ConfusionSpinOff).
/// </summary>
public class Confusion(): SlalTheEndlessPower
{
  public override PowerType Type => PowerType.Debuff;
  public override PowerStackType StackType => PowerStackType.Counter;
  public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;

  private int HpLossAmount => Math.Max(0, Math.Min(Amount, Owner.CurrentHp - 1));

  private static bool IsAttack(Creature? dealer, ValueProp props) =>
    dealer != null && props.IsPoweredAttack();

  public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
  {
    if (!participants.Contains(Owner)) return;

    var owner = Owner;
    var damageAmount = HpLossAmount;
    if (damageAmount > 0) Flash();
    // Remove before dealing damage so the health bar forecast doesn't redraw against the lowered HP mid-hit
    await PowerCmd.Remove(this);
    if (damageAmount > 0)
    {
      await CreatureCmd.Damage(
        choiceContext,
        owner,
        damageAmount,
        ValueProp.Unblockable | ValueProp.Unpowered,
        null,
        null
      );
    }
  }

  public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
  {
    if (!Owner.IsAlive || !IsAttack(command.Attacker, command.DamageProps)) return;
    if (!command.Results.SelectMany(r => r).Any(r => r.Receiver == Owner)) return;
    if (Amount < Owner.CurrentHp) return;

    Flash();
    // Only reached when the attack didn't kill it by itself
    await ConfusionSpinOff.Kill(Owner);
  }

  public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(
    HealthBarForecastContext context)
  {
    var segment = new HealthBarForecastSegment
    {
      Amount = HpLossAmount,
      Color = new Color("#40E0D0", 1f),
      Direction = HealthBarForecastDirection.FromRight,
      Order = 0,
    };

    yield return segment;
  }
}
