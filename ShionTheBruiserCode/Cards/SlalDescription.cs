using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalDescription() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Token,
    TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromCard<SlalConclusion>(IsUpgraded),
        HoverTipFactory.FromCard<SlalBumperForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalVentilatorForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalSqueakySquirrelForm>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var random = SlalMechanicsHelper.GetRandom(card.Owner.Creature);
        if (random > 0)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, random, card.Owner.Creature, card);
            await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, random, card.Owner.Creature, card);
        }

        List<CardModel> cards = [
            SlalMechanicsHelper.CreateCard<SlalConclusion>(card.Owner, IsUpgraded),
            SlalMechanicsHelper.CreateCard<SlalBumperForm>(card.Owner, IsUpgraded),
            SlalMechanicsHelper.CreateCard<SlalVentilatorForm>(card.Owner, IsUpgraded),
            SlalMechanicsHelper.CreateCard<SlalSqueakySquirrelForm>(card.Owner, IsUpgraded),
        ];
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
