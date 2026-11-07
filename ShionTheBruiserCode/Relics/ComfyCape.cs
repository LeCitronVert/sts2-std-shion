using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Healing from Rest Sites heals an additional 20% of your Max HP.
/// Works on Mend too: Mend heals its target through the same hook.
/// </summary>
public class ComfyCape : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Percent", 20m)];

    public override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
    {
        if (creature.Player != Owner) return amount;
        return amount + Math.Floor(creature.MaxHp * DynamicVars["Percent"].BaseValue / 100m);
    }
}
