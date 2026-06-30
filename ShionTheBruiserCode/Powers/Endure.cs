using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Endure() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(50),
        new DynamicVar("DebuffAmount", 5)
    ];
    
    public override bool ShouldDieLate(Creature creature)
    {
        return creature != this.Owner;
    }
    
    public override async Task AfterPreventingDeath(Creature creature)
    {
        var power = this;
        power.Flash();
        await CreatureCmd.Heal(creature, Math.Max(1M, (Decimal) creature.MaxHp * (power.DynamicVars.Heal.BaseValue / 100M)));
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, -DynamicVars["DebuffAmount"].BaseValue, Owner, null!);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner, -DynamicVars["DebuffAmount"].BaseValue, Owner, null!);
        await PowerCmd.Decrement(power);
    }
}