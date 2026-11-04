using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Triggers Maim X on every damage instance that makes an enemy lose HP, whoever owns the card.
/// </summary>
public class MaimSingleton() : CustomSingletonModel(HookType.Combat)
{
    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return MaimHelper.OnDamageReceived(choiceContext, target, result.UnblockedDamage, cardSource);
    }
}
