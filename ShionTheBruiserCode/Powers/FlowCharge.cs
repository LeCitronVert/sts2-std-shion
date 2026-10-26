using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class FlowCharge : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public static readonly int VigorAmount = 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VigorPower>(VigorAmount),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            await ApplyPower();
        }
    }

    /// <summary>
    /// Flow Charge+ gives more Vigor per turn. Stacks share one per-turn amount, so the strongest one applied wins. //todo : stacking rule not specified
    /// </summary>
    public void RaiseVigorAmount(decimal vigorAmount)
    {
        var vigor = DynamicVars["VigorPower"];
        if (vigorAmount > vigor.BaseValue) vigor.BaseValue = vigorAmount;
    }

    public async Task ApplyPower()
    {
        var power = this;
        await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), power.Owner, power.DynamicVars["VigorPower"].BaseValue, power.Owner, null!);
        await PowerCmd.TickDownDuration(this);
    }
}