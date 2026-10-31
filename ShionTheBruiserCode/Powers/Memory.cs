using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of your turn, gain X Flashback. Cards Flashback plays that apply Confusion apply X more.
/// Not written: whenever you gain Memory, also gain that much Flashback right away (see <see cref="SlalMechanicsHelper.GainMemory"/>).
/// </summary>
public class Memory : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is not Confusion || amount <= 0 || cardSource?.Owner?.Creature != Owner) return 0m;
        return SlalMechanicsHelper.GetMemoryConfusionBonus(cardSource);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        Flash();
        await SlalMechanicsHelper.GainFlashback(choiceContext, Owner, Amount);
    }
}
