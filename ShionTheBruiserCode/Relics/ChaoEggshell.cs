using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class ChaoEggshell() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    private int _attacksReceivedThisTurn;
    public const int ReductionAmount = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ReductionAmount", ReductionAmount)
    ];
    
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature))
            return Task.CompletedTask;
        this._attacksReceivedThisTurn = 0;
        this.Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        this._attacksReceivedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature) return 0;
        
        return -(ReductionAmount * _attacksReceivedThisTurn);
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return Task.CompletedTask;
        
        // Only count attacks from other creatures (not self-damage)
        if (dealer != null && dealer != Owner.Creature)
        {
            if (_attacksReceivedThisTurn > 0)
            {
                Flash();
            }
            _attacksReceivedThisTurn++;
        }
        
        return Task.CompletedTask;
    }

}