using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you Gamble, draw X cards.
/// </summary>
public class CliffDropper : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble)];

    public async Task AfterGamble(PlayerChoiceContext choiceContext, Player player, int amount, IReadOnlyList<CardModel> cards)
    {
        if (player != Owner.Player) return;

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
