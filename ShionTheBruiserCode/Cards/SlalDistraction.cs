using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Shapeshift (upgraded: Shapeshift+). If you are Weak, Shapeshift (Shapeshift+) again. Exhaust.
/// Weak is checked before the first Shapeshift.
/// </summary>
public class SlalDistraction(): SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(IsUpgraded ? SlalCardKeywords.ShapeshiftPlus : SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.HasPower<WeakPower>();

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var count = Owner.Creature.HasPower<WeakPower>() ? 2 : 1;
        await SlalMechanicsHelper.Shapeshift(choiceContext, Owner, count, IsUpgraded);
    }

    protected override void OnUpgrade()
    {
    }
}
