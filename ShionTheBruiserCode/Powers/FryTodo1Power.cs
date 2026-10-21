using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever any other player ends their turn, gain Stars equal to their remaining energy,
/// and ALL players gain the same amount of Block next turn.
/// </summary>
public class FryTodo1Power : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlockNextTurnPower>()];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || Owner.CombatState is null || Owner.Player is null) return;

        var players = Owner.CombatState.GetTeammatesOf(Owner).Where(c => c.IsPlayer && c.IsAlive).ToList();
        var energy = players
            .Where(c => c != Owner)
            .Sum(c => c.Player?.PlayerCombatState?.Energy ?? 0) * Amount;
        if (energy <= 0) return;

        Flash();
        await PlayerCmd.GainStars(energy, Owner.Player);
        foreach (var player in players)
        {
            await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, player, energy, Owner, null);
        }
    }
}
