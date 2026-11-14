using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Expy (Team-Up), on the Partner who Summoned the copy of the Necrobinder's Osty. Amount = HP that copy was Summoned with.
/// At the start of the players' next turn (after the enemy turn), the copy goes away. If it's still alive, the Necrobinder
/// Summons its remaining HP (capped at Amount).
/// If the Partner already had an Osty, the copy only added Max HP to it, so only that Max HP is taken back.
/// </summary>
public class Expy : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;

        var amount = Amount;
        var necrobinder = LivingPartners.FirstOrDefault()?.Player;
        await PowerCmd.Remove(this);

        if (Owner.Player is not { IsOstyAlive: true } owner) return;
        var osty = owner.Osty!;
        var remaining = Math.Min(amount, osty.CurrentHp);

        if (osty.MaxHp <= amount)
        {
            await CreatureCmd.Kill(osty);
        }
        else
        {
            await Sts2Compat.LoseMaxHp(choiceContext, osty, amount, null);
        }

        if (necrobinder != null && remaining > 0)
        {
            await OstyCmd.Summon(choiceContext, necrobinder, remaining, this);
        }
    }
}
