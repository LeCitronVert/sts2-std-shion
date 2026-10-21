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
/// Whenever you Blood Bend X, ALL players gain X Block, and their Block is not removed for the next (Amount) turns.
/// </summary>
public class SplashArmor : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlurPower>()];

    public async Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (player != Owner.Player || x <= 0 || Owner.CombatState is null) return;

        Flash();
        foreach (var ally in Owner.CombatState.GetTeammatesOf(Owner).Where(c => c.IsPlayer && c.IsAlive).ToList())
        {
            await CreatureCmd.GainBlock(ally, x, ValueProp.Unpowered, null);
            await PowerCmd.Apply<BlurPower>(choiceContext, ally, Amount, Owner, null);
        }
    }
}
