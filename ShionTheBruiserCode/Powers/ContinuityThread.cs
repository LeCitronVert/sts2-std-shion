using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// You choose the cards Memory plays from your Discard Pile instead of picking them at random.
/// </summary>
public class ContinuityThread : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
    ];

    public async Task<List<CardModel>> SelectCardsForMemory(PlayerChoiceContext choiceContext, Player player, int count, Func<CardModel, bool> filter)
    {
        Flash();
        return (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Discard.GetPile(player),
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, count),
            filter
        )).ToList();
    }
}
