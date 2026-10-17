using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalThinkingBoard() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalMaybeLater>(),
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
            var maybeLater = SlalMechanicsHelper.CreateCard<SlalMaybeLater>(card.Owner);
            SlalMechanicsHelper.MakeSteady(maybeLater);
            await CardPileCmd.AddGeneratedCardToCombat(maybeLater, PileType.Hand, card.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
