using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class ChoiceBand() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    private int _attacksUsedThisTurn = 0;
    public const int VigorAmount = 10;
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    
    public override int DisplayAmount
    {
        get
        {
            return _attacksUsedThisTurn;
        }
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("VigorAmount", VigorAmount)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];
    
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack)
            return Task.CompletedTask;
        
        _attacksUsedThisTurn++;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(this.Owner.Creature))
            return Task.CompletedTask;

        var attacksUsed = _attacksUsedThisTurn;
        _attacksUsedThisTurn = 0;
        
        if (attacksUsed != 1) return Task.CompletedTask;
        
        PowerCmd.Apply<VigorPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            amount: VigorAmount,
            applier: Owner.Creature,
            cardSource: null);
        
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _attacksUsedThisTurn = 0;
        return Task.CompletedTask;
    }
    
    private void UpdateDisplay()
    {
        this.Status = _attacksUsedThisTurn == 1 ? RelicStatus.Active : RelicStatus.Normal;
        this.InvokeDisplayAmountChanged();
    }
}