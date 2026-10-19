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
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GuardAmount", BreakMeIfYouCan.GuardAmount),
        new DynamicVar("VigorAmount", BreakMeIfYouCan.VigorAmount),
        new DynamicVar("BlockAmount", BreakMeIfYouCan.BlockAmount),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, ShionCardKeywords.Defensive];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await BreakMeIfYouCan.GainBonuses(choiceContext, card.Owner.Creature);
        await PowerCmd.Apply<BreakMeIfYouCan>(choiceContext, card.Owner.Creature, 1M, card.Owner.Creature, card);
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
