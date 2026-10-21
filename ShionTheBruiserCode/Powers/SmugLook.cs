using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// For X turns, whenever you play a card, deal 2 damage to a random enemy.
/// </summary>
public class SmugLook : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2M, ValueProp.Unpowered)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || Owner.CombatState is null) return;

        var enemies = Owner.CombatState.HittableEnemies.ToList();
        var target = Owner.Player!.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target is null) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, Owner, null);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        return side == Owner.Side ? PowerCmd.TickDownDuration(this) : Task.CompletedTask;
    }
}
