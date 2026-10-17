using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalKickstartEvent() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(10),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromCard<SlalFreshIdea>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Steady>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, card.DynamicVars["Confusion"].BaseValue, card);

        var amount = CardPile.MaxCardsInHand - PileType.Hand.GetPile(card.Owner).Cards.Count;
        List<CardModel> ideas = [];
        for (var i = 0; i < amount; i++)
        {
            var idea = SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded);
            SlalMechanicsHelper.MakeSteady(idea);
            ideas.Add(idea);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(ideas, PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
