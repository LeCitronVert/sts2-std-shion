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
    
    public override Decimal ModifyHpLostAfterOsty(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature) return amount;
        var reduction = ReductionAmount * _attacksReceivedThisTurn;
        var damage = Math.Max(0M, amount - 1 * reduction);
        _attacksReceivedThisTurn++;

        if (reduction > 0)
        {
            Flash();
        }
        
        return damage;
    }
}