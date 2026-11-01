using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Legion of Death (Necrobinder Team Blast), on an enemy: whenever it receives attack damage, it gains that much Doom.
/// Lasts Amount player turns (it ticks down when the players' turn ends).
/// </summary>
public class LegionOfDeath : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || Owner.IsDead || !props.IsPoweredAttack() || result.UnblockedDamage <= 0) return;

        Flash();
        await PowerCmd.Apply<DoomPower>(choiceContext, Owner, result.UnblockedDamage, dealer, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        await PowerCmd.Remove(this);
    }
}
