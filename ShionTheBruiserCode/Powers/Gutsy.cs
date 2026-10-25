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
/// Whenever you are hit by an Attack, gain X Vigor next turn (triple if it made you lose HP).
/// </summary>
public class Gutsy : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public const int HpLossMultiplier = 3;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VigorNextTurn>(),
    ];

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? _,
        CardModel? __)
    {
        if (target != Owner || !props.IsPoweredAttack())
            return;

        var vigorAmount = result.UnblockedDamage > 0 ? Amount * HpLossMultiplier : Amount;
        if (vigorAmount <= 0) return;

        // Can trigger mid-attack, so it goes through VigorNextTurn: adding VigorPower while
        // it's tracking an attack leaves it stuck and never applied again.
        Flash();
        await PowerCmd.Apply<VigorNextTurn>(
            choiceContext,
            Owner,
            vigorAmount,
            Owner,
            null
        );
    }
}
