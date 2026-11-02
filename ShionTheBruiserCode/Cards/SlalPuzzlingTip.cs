using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 2 Weak. Apply 5 Confusion. Choose to Mimic 2 or Shapeshift (upgraded: and upgrade that card, i.e. the offered cards are upgraded).
/// Not written: when Flashback plays it, it always Shapeshifts (see <see cref="SlalMechanicsHelper.MimicOrShapeshift"/>).
/// </summary>
public class SlalPuzzlingTip() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
        new PowerVar<Confusion>(5),
        new CardsVar(2),
    ];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift, SlalCardTags.Mimic];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
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
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);
        await SlalMechanicsHelper.MimicOrShapeshift(choiceContext, card.Owner, card.DynamicVars.Cards.IntValue, IsUpgraded, card);
    }

    protected override void OnUpgrade()
    {
    }
}
