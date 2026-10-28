using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Fellowship (Team-Up), on both players: whenever you play a card, your Partner gains Amount Scrappy.
/// </summary>
public class Fellowship : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Scrappy>()];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner) return;

        foreach (var partner in LivingPartners.ToList())
        {
            Flash();
            await PowerCmd.Apply<Scrappy>(choiceContext, partner, Amount, Owner, null);
        }
    }
}
