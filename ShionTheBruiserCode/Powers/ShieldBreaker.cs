using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class ShieldBreaker : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return card.Owner.Creature == this.Owner && card.Type == CardType.Attack && target?.Block > 0 ? playCount + Amount : playCount;
    }
}


