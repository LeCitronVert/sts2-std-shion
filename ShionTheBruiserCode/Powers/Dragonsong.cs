using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Dragonsong : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyHpLostAfterOsty(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner) return amount;
        var amountOfBuffs = Math.Min(ShionMechanicsHelper.GetDragonUniqueBuffCount(target), Amount);

        // Visual only. ModifyHpLost* is only called by real damage, never by previews
        if (amountOfBuffs > 0 && amount > 0) ShionVfx.FlashDragonsong(target);

        return Math.Max(0M, amount - amountOfBuffs);
    }
}