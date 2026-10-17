using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Fresh Idea, add a Maybe Later into your Hand and gain X energy.
/// </summary>
public class StreetLightForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(),
        HoverTipFactory.FromCard<SlalMaybeLater>(),
        HoverTipFactory.ForEnergy(this),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card is not SlalFreshIdea) return;

        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalMaybeLater>(Owner.Player), PileType.Hand, Owner.Player);
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
    }
}
