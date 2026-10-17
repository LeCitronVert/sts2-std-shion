using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Applied by Anchor Throw. Amount is the number of turns left. At the start of the player's turn when it reaches 0,
/// apply the stored Confusion to the owner (which then triggers at the end of that turn, like The Bomb).
/// </summary>
public class AnchoredDown : SlalTheEndlessPower
{
    public override string CustomPackedIconPath => "anchor_throw_2.png".PowerImagePath();
    public override string CustomBigIconPath => "anchor_throw_2.png".BigPowerImagePath();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Confusion", 0)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    public void SetConfusion(decimal amount)
    {
        AssertMutable();
        DynamicVars["Confusion"].BaseValue = amount;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        var confusion = DynamicVars["Confusion"].BaseValue;
        if (confusion > 0 && Owner.IsAlive)
        {
            Flash();
            await PowerCmd.Apply<Confusion>(new ThrowingPlayerChoiceContext(), Owner, confusion, Applier, null);
        }
        await PowerCmd.Remove(this);
    }
}
