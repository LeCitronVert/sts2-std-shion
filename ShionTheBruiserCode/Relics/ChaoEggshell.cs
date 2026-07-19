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
public class ChaoEggshell : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    
    private int _attacksReceivedThisTurn;
    public const int ReductionAmount = 1;
    
    private int AttacksReceivedThisTurn
    {
        get => _attacksReceivedThisTurn;
        set
        {
            AssertMutable();
            _attacksReceivedThisTurn = value;
            UpdateDisplay();
        }
    }
    
    public override int DisplayAmount => AttacksReceivedThisTurn;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ReductionAmount", ReductionAmount)
    ];
    
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(this.Owner.Creature))
        {
            AttacksReceivedThisTurn = 0;
        }
        return Task.CompletedTask;
    }
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        AttacksReceivedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return Task.CompletedTask;
        
        AttacksReceivedThisTurn++;
        
        return Task.CompletedTask;
    }
    
    public override Decimal ModifyHpLostAfterOsty(
        Creature target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return target != this.Owner.Creature ? amount : Math.Max(0M, amount - ReductionAmount * _attacksReceivedThisTurn);
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        this.Flash();
        return Task.CompletedTask;
    }
    
    private void UpdateDisplay()
    {
        this.Status = _attacksReceivedThisTurn > 0 ? RelicStatus.Active : RelicStatus.Normal;
        this.InvokeDisplayAmountChanged();
    }
}
