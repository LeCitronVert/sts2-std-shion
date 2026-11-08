using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Flashback. Choose to Mimic 2 or Shapeshift (upgraded: and upgrade that card, i.e. the offered cards are upgraded).
/// Not written: when Flashback plays it, it always Shapeshifts. Exhaust.
/// </summary>
public class SlalPlotTwist() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Flashback>(1),
        new CardsVar(2),
    ];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift, SlalCardTags.Mimic];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.GainFlashback(choiceContext, card.Owner.Creature, card.DynamicVars["Flashback"].BaseValue, card);
        await SlalMechanicsHelper.MimicOrShapeshift(choiceContext, card.Owner, card.DynamicVars.Cards.IntValue, IsUpgraded, card);
    }

    protected override void OnUpgrade()
    {
    }
}
