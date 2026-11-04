using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Triggers Maim X once per attack for each enemy that lost HP from it (multi-hit attacks don't trigger it per hit),
/// and once per HP loss instance for non-attack sources ("an enemy loses X HP"). Works whoever owns the card.
/// </summary>
public class MaimSingleton() : CustomSingletonModel(HookType.Combat)
{
    public override Task BeforeAttack(AttackCommand command)
    {
        MaimHelper.OnAttackStarted(command);
        return Task.CompletedTask;
    }

    public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        return MaimHelper.OnAttackFinished(choiceContext, command);
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return MaimHelper.OnDamageReceived(choiceContext, target, result.UnblockedDamage, cardSource);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        MaimHelper.Reset();
        return Task.CompletedTask;
    }
}
