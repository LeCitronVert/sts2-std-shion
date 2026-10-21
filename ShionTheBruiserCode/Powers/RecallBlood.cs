using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you Blood Bend X, ALL enemies lose X HP, and you gain X Vigor next turn.
/// </summary>
public class RecallBlood : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

    public async Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (player != Owner.Player || x <= 0 || Owner.CombatState is null) return;

        Flash();
        var amount = x * Amount;
        await CreatureCmd.Damage(choiceContext, Owner.CombatState.HittableEnemies.ToList(), amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner, null);
        await PowerCmd.Apply<VigorNextTurn>(choiceContext, Owner, amount, Owner, null);
    }
}
