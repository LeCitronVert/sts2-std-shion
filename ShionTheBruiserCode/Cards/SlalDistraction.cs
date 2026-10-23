using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add a random (upgraded) Goofball Token Power into your Hand. Exhaust.
/// </summary>
public class SlalDistraction(): SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var power = SlalMechanicsHelper.CreateGoofballTokenPower(card.Owner, IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(power, PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
