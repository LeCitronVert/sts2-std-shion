using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(ShionTheBruiserRelicPool))]
public class SpikesGloves() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2, ValueProp.Unpowered)
    ];
    
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
            || (!cardSource.Tags.Contains(ShionCardTags.Energy) && !cardSource.Tags.Contains(CardTag.Strike)) 
            || dealer != this.Owner.Creature
            && cardSource.Owner.Creature != this.Owner.Creature ? 0M : DynamicVars.Damage.BaseValue
        ;
    }
}