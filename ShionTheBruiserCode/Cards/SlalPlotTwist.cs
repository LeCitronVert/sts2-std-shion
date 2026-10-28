using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 3 Confusion. Choose between 2 non-Rare cards from other characters and a Goofball Token Power to add into your Hand
/// (upgraded: they are upgraded). Exhaust.
/// Creative Spirit allows Rare cards, Mood Shift allows Colorless cards.
/// </summary>
public class SlalPlotTwist() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(3),
        new CardsVar(2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);

        var options = SlalMechanicsHelper.GetCharacterCardOptions(card.Owner, includeOwnCharacter: false)
            .Where(c => SlalMechanicsHelper.HasRandomCardRarity(card.Owner, c));
        await SlalMechanicsHelper.ChooseCardOrGoofballToAddToHand(choiceContext, card.Owner, options, card.DynamicVars.Cards.IntValue, IsUpgraded);
    }

    protected override void OnUpgrade()
    {
    }
}
