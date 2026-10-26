using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you create a card, play it (X times).
/// Cards created while Sparrow Form is already playing a card are not played, to avoid infinite loops
/// (e.g. Running Joke creating copies of itself).
/// </summary>
public class SparrowForm : SlalTheEndlessPower
{
    private static int _autoPlayDepth;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (creator == null || creator != Owner.Player || card.Owner != Owner.Player) return;
        if (_autoPlayDepth > 0) return;
        if (card.Keywords.Contains(CardKeyword.Unplayable)) return;

        Flash();
        _autoPlayDepth++;
        try
        {
            for (var i = 0; i < Amount; i++)
            {
                // The card may have been exhausted or removed by its first play.
                if (card.Pile == null || card.Pile.Type is PileType.Exhaust or PileType.None) break;
                // Created cards can ask for a choice (e.g. Creative Break, Fresh Idea+), which a ThrowingPlayerChoiceContext crashes on.
                await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null);
            }
        }
        finally
        {
            _autoPlayDepth--;
        }
    }
}
