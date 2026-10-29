using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Slimy. Gain 11 Block. Gain 2 Weak. Ignore your Vulnerable until your next turn. Exhaust. (Lose Exhaust)
/// </summary>
public class SlalFavouriteBucket(): SlalSlimyCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(11M, ValueProp.Move),
        new PowerVar<WeakPower>(2),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        SlalCardKeywords.Slimy,
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<IgnoreVulnerable>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<IgnoreVulnerable>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
