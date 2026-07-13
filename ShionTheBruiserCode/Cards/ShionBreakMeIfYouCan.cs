using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBreakMeIfYouCan() : ShionDefensiveCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int PlatingInitialAmount = 7;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("PlatingAmount", BreakMeIfYouCan.PlatingAmount),
        new DynamicVar("PlatingInitialAmount", PlatingInitialAmount),
        new DynamicVar("VigorAmount", BreakMeIfYouCan.VigorAmount)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, ShionCardKeywords.Defensive];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<PlatingPower>(choiceContext, card.Owner.Creature, PlatingInitialAmount, card.Owner.Creature, card);
        await PowerCmd.Apply<BreakMeIfYouCan>(choiceContext, card.Owner.Creature, 1M, card.Owner.Creature, card);
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}