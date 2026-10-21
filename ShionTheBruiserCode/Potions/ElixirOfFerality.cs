using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Put ALL your Exhausted Slashes on top of your Draw Pile.
/// </summary>
public class ElixirOfFerality : SeelsTheRecklessPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SeelsSlash>()];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var slashes = PileType.Exhaust.GetPile(Owner).Cards.Where(SeelsMechanicsHelper.IsSlash).ToList();
        if (slashes.Count == 0) return;

        await CardPileCmd.Add(slashes, PileType.Draw, CardPilePosition.Top);
    }
}
