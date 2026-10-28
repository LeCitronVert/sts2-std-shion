using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the end of your turn, play X random colored non-Novel cards from your Discard Pile.
/// Also plays the Memory This Turn amount in the same pick, so no card is played twice.
/// Cards it plays apply X more Confusion (X = the Memory amount used).
/// </summary>
public class Memory : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is not Confusion || amount <= 0 || cardSource?.Owner?.Creature != Owner) return 0m;
        return SlalMechanicsHelper.GetMemoryConfusionBonus(cardSource);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is null) return;

        Flash();
        await SlalMechanicsHelper.PlayFromMemory(choiceContext, Owner.Player, SlalMechanicsHelper.GetMemory(Owner));
    }
}
