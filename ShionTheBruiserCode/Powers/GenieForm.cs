using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you reach 10 Random, apply 1 Weak to a random enemy (X times).
/// Cards you create can be of any color (see <see cref="SlalMechanicsHelper.GetRandomCardOptions"/>).
/// </summary>
public class GenieForm : SlalTheEndlessPower, ISlalRandomListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Random>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    public async Task AfterRandomThresholdReached(PlayerChoiceContext choiceContext, Creature gainer)
    {
        if (gainer != Owner || CombatState is null) return;

        Flash();
        for (var i = 0; i < Amount; i++)
        {
            var enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            if (enemy == null) break;
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 1, Owner, null);
        }
    }
}
