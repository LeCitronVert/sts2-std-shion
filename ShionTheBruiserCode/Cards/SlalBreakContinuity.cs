using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Whenever you play a Novel card, Free Shapeshift (once), gain 1 Frail and 1 Disintegration. (Innate)
/// </summary>
public class SlalBreakContinuity() : SlalTheEndlessCard(2,
    CardType.Power, CardRarity.Ancient,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromKeyword(SlalCardKeywords.FreeShapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
        HoverTipFactory.FromPower<FrailPower>(),
        HoverTipFactory.FromPower<DisintegrationPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<BreakContinuity>(1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<BreakContinuity>(choiceContext, card.Owner.Creature, card.DynamicVars["BreakContinuity"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
