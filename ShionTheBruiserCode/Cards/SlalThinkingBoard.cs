using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add X (X+1) Steady Fresh Idea+ into your Hand.
/// </summary>
public class SlalThinkingBoard() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(true),
        ..HoverTipFactory.FromEnchantment<Steady>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var amount = ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);
        for (var i = 0; i < amount; i++)
        {
            var freshIdea = SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, true);
            SlalMechanicsHelper.MakeSteady(freshIdea);
            await CardPileCmd.AddGeneratedCardToCombat(freshIdea, PileType.Hand, card.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
