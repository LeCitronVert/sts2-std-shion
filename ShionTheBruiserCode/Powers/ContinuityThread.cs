using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Next turn, put X cards from your Discard Pile into your Hand (like Foregone Conclusion).
/// </summary>
public class ContinuityThread : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner.Player) return;

        var cards = await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Discard.GetPile(player),
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, Amount)
        );
        await CardPileCmd.Add(cards, PileType.Hand);
        await PowerCmd.Remove(this);
    }
}
