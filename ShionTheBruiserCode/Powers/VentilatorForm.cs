using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times you play a Power each turn, ALL enemies lose 1 Strength this turn.
/// </summary>
public class VentilatorForm : SlalFirstPlaysEachTurnPower
{
    public const int StrengthLoss = 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override bool ShouldTrigger(CardPlay cardPlay) => cardPlay.Card.Type == CardType.Power;

    protected override async Task OnTriggered(PlayerChoiceContext choiceContext, CardPlay cardPlay, Player player)
    {
        if (CombatState is null) return;
        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<VentilatorFormStrengthDown>(choiceContext, enemy, StrengthLoss, Owner, null);
        }
    }
}
