using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The next X times you draw an Erratic card, gain Intangible equal to its randomized energy cost.
/// </summary>
public class GamblingAddiction : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
        HoverTipFactory.FromPower<IntangiblePower>(),
    ];

    public async Task AfterErraticCostRandomized(PlayerChoiceContext choiceContext, CardModel card, int cost)
    {
        if (card.Owner.Creature != Owner) return;

        Flash();
        if (cost > 0)
        {
            await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner, cost, Owner, null);
        }
        await PowerCmd.Decrement(this);
    }
}
