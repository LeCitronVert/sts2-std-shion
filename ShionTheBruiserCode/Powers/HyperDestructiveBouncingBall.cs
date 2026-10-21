using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of your turn, deal 3 damage to a random enemy X times, then increase the number of hits by 2.
/// </summary>
public class HyperDestructiveBouncingBall : FryTheCommanderPower
{
    public const int Damage = 3;
    public const int HitIncrease = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || Owner.CombatState is null) return;

        Flash();
        for (var i = 0; i < Amount; i++)
        {
            var enemies = Owner.CombatState.HittableEnemies.ToList();
            var target = player.RunState.Rng.CombatTargets.NextItem(enemies);
            if (target is null) break;
            await CreatureCmd.Damage(choiceContext, target, Damage, ValueProp.Unpowered, Owner, null);
        }

        await PowerCmd.ModifyAmount(choiceContext, this, HitIncrease, Owner, null);
    }
}
