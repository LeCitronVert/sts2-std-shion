using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Add 2 upgraded Glass 2 Slashes into your Hand.
/// </summary>
public class EmptyBottle : SeelsTheRecklessPotion
{
    public const int GlassAmount = 2;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(true),
        ..HoverTipFactory.FromEnchantment<Glass>(GlassAmount),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var slashes = Enumerable.Range(0, DynamicVars.Cards.IntValue).Select(_ =>
        {
            var slash = SeelsMechanicsHelper.CreateSlash(Owner, true);
            SeelsMechanicsHelper.Enchant<Glass>(slash, GlassAmount);
            return slash;
        }).ToList();
        await CardPileCmd.AddGeneratedCardsToCombat(slashes, PileType.Hand, Owner);
    }
}
