using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionTouchOfForesight() : ShionDefensiveCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        ShionCardKeywords.Defensive,
        CardKeyword.Ethereal,
        CardKeyword.Exhaust,
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<TemporaryBuffer>(1),
        new PowerVar<CounterStrategy>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TemporaryBuffer>(),
        HoverTipFactory.FromPower<CounterStrategy>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Defensive),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<TemporaryBuffer>(choiceContext, card.Owner.Creature, card.DynamicVars["TemporaryBuffer"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<CounterStrategy>(choiceContext, card.Owner.Creature, card.DynamicVars["CounterStrategy"].BaseValue, card.Owner.Creature, card);
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
