using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Until your next turn, Vulnerable doesn't increase the Attack damage you take.
/// Given by Favourite Bucket and Instant Mattress.
/// </summary>
public class IgnoreVulnerable : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return 1m;

        // Cancels out Vulnerable's own multiplier (which already includes Paper Phrog, Cruelty and Debilitate)
        var vulnerable = Owner.GetPower<VulnerablePower>();
        if (vulnerable is null) return 1m;

        var multiplier = vulnerable.ModifyDamageMultiplicative(target, amount, props, dealer, cardSource);
        return multiplier > 0m ? 1m / multiplier : 1m;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return;

        await PowerCmd.Remove(this);
    }
}
