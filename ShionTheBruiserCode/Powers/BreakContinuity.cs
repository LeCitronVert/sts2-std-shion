using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// This combat, Memory doesn't play cards (see <see cref="SlalMechanicsHelper.PlayFromMemory"/>).
/// Instead, the first X cards you play each turn (X = your Memory) give Y energy and draw Y cards.
/// </summary>
public class BreakContinuity : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !cardPlay.IsFirstInSeries || Owner.Player is null || CombatState is null) return;

        // This play is already in the history
        var cardsPlayedThisTurn = CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Owner == Owner.Player && e.CardPlay.IsFirstInSeries);
        if (cardsPlayedThisTurn > SlalMechanicsHelper.GetMemory(Owner)) return;

        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
