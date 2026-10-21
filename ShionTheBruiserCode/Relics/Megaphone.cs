using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// While Vulnerable, you have 2 additional Dexterity.
/// </summary>
public class Megaphone : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(2M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        var creature = Owner.Creature;
        if (cardSource != null ? cardSource.Owner.Creature != creature : target != creature) return 0m;
        if (!props.IsPoweredCardOrMonsterMoveBlock() || !creature.HasPower<VulnerablePower>()) return 0m;

        return DynamicVars.Dexterity.BaseValue;
    }
}
