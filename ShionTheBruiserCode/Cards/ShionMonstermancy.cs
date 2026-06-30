using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionMonstermancy() : ShionTheBruiserCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<Monstermancy>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}