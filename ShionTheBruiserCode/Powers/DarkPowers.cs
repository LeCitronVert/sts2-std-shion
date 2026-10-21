using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times you play a Slash each turn, gain 1 energy and apply 1 Vulnerable to a random enemy.
/// </summary>
public class DarkPowers : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    private int _triggersThisTurn;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !SeelsMechanicsHelper.IsSlash(cardPlay.Card)) return;
        if (_triggersThisTurn >= Amount || Owner.CombatState is null) return;

        _triggersThisTurn++;
        Flash();
        await PlayerCmd.GainEnergy(1, Owner.Player!);

        var target = Owner.Player!.RunState.Rng.CombatTargets.NextItem(Owner.CombatState.HittableEnemies.ToList());
        if (target is not null)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1, Owner, null);
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side) _triggersThisTurn = 0;
        return Task.CompletedTask;
    }
}
