using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of your turn, if you have 8 Weak or more, lose 4 Weak and gain X Thorns.
/// </summary>
public class CactusForm : SlalTheEndlessPower
{
    public const int WeakThreshold = 8;
    public const int WeakLost = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<ThornsPower>(),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        var weak = Owner.GetPower<WeakPower>();
        if (weak == null || weak.Amount < WeakThreshold) return;

        Flash();
        await PowerCmd.ModifyAmount(choiceContext, weak, -WeakLost, Owner, null);
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
