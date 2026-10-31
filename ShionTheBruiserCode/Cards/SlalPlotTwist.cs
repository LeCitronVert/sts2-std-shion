using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 3 Confusion. Choose to Mimic 2 or Shapeshift (upgraded: the offered cards are upgraded). Exhaust.
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

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift, SlalCardTags.Mimic];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);

        await SlalMechanicsHelper.MimicOrShapeshift(choiceContext, card.Owner, card.DynamicVars.Cards.IntValue, IsUpgraded);
    }

    protected override void OnUpgrade()
    {
    }
}
