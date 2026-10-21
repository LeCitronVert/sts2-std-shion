using BaseLib.Hooks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the end of your turn, lose X HP, then this is removed. Refuse prevents it from triggering.
/// </summary>
public class DamageDebt : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Refuse>()];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.HasPower<Refuse>()) return;

        Flash();
        var amount = Amount;
        await PowerCmd.Remove(this);
        await FryMechanicsHelper.PayDebt(choiceContext, Owner, amount);
    }

    public override IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (Owner.HasPower<Refuse>()) yield break;

        yield return new HealthBarForecastSegment
        {
            Amount = Math.Min(Amount, Owner.CurrentHp),
            Color = new Color("#FF8C00", 1f),
            Direction = HealthBarForecastDirection.FromRight,
            Order = 0,
        };
    }
}
