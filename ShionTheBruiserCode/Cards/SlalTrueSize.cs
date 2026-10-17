using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalTrueSize() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Slimed>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var amount = ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);
        for (var i = 0; i < amount; i++)
        {
            var slimed = SlalMechanicsHelper.CreateCard<Slimed>(card.Owner);
            slimed.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Hand, card.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
