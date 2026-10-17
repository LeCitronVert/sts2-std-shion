using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Raise your Random until it reaches 9.
/// </summary>
public class CottonCandy : SlalTheEndlessPotion
{
    public const int TargetRandom = 9;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Random>(),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        // Random is displayed from 1 to 10 (0 without the power), so count up to the next 9 without crossing a 10.
        var current = SlalMechanicsHelper.GetRandom(Owner.Creature);
        var amount = (Random.GetCardAt + TargetRandom - current) % Random.GetCardAt;

        await SlalMechanicsHelper.GainRandom(choiceContext, Owner.Creature, amount);
    }
}
