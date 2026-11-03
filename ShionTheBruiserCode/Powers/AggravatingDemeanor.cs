using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever your Gloat triggers (you gain energy from it), immediately gain as much Blur and X Block.
/// The amount is the Block; stacking adds Block, the Blur always equals the Gloat energy.
/// </summary>
public class AggravatingDemeanor : SeelsTheRecklessPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Gloat>(),
        HoverTipFactory.FromPower<BlurPower>(),
    ];

    public async Task AfterGloatTriggered(PlayerChoiceContext choiceContext, Creature creature, int energy)
    {
        if (creature != Owner) return;

        Flash();
        if (energy > 0)
        {
            await PowerCmd.Apply<BlurPower>(choiceContext, Owner, energy, Owner, null);
        }
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
