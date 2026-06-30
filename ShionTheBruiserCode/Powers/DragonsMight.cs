using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class DragonsMight : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Decimal ModifyDamageAdditive(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return 
            !props.IsPoweredAttack() 
            || cardSource == null 
            || !cardSource.Tags.Contains(CardTag.Strike) 
            || dealer != this.Owner 
            && cardSource.Owner.Creature != this.Owner ? 0M : amount * ShionMechanicsHelper.GetDragonUniqueBuffCount(Owner)
        ;
    }
}