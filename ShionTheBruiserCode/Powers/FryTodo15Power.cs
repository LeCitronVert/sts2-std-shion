using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Each of your Requested counters gives you X Dexterity until it reaches 0.
/// </summary>
public class FryTodo15Power : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Requested>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null ? cardSource.Owner.Creature != Owner : target != Owner) return 0m;
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 0m;

        return FryMechanicsHelper.GetRequests(Owner).Count * Amount;
    }
}
