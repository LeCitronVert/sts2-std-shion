using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Every 4 energy you spend, gain X Blood.
/// </summary>
public class SweatAndTears : SeelsTheRecklessPower
{
    public const int EnergyPerTrigger = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    private int _energySpent;

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (card.Owner.Creature != Owner || amount <= 0) return;

        _energySpent += amount;
        var triggers = _energySpent / EnergyPerTrigger;
        _energySpent %= EnergyPerTrigger;
        if (triggers <= 0) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(new ThrowingPlayerChoiceContext(), Owner, triggers * Amount);
    }
}
