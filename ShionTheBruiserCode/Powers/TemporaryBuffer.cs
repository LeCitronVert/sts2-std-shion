using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Buffer that only lasts until your next turn.
/// It prevents HP loss in the AfterOsty phase, before the regular Buffer (AfterOstyLate),
/// so it is always used up first and the regular Buffer isn't decremented for the same hit.
/// </summary>
public class TemporaryBuffer : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BufferPower>(),
    ];

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return target != Owner ? amount : 0m;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return;

        await PowerCmd.Remove(this);
    }
}
