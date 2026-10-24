using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class NoHealPower() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta <= 0M)
            return;

        Flash();
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner,
            new DamageVar(delta, ValueProp.Unblockable | ValueProp.Unpowered),
            null,
            null
        );
    }
}