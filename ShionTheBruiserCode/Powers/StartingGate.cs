using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class StartingGate() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public int AmountThisTurn = 0;
    public bool Initialized;
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        AmountThisTurn = Amount;
        Initialized = true;
    }
    
    public void AdaptAmountAsNeeded(int amount)
    {
        if (Initialized) AmountThisTurn += amount;
        else Initialize();
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != this.Owner || ShionMechanicsHelper.IsCardNativeToCharacter(card, card.Owner.Character) || AmountThisTurn <= 0)
            return false;
        
        PileType? type = card.Pile?.Type;
        bool shouldModify = type.HasValue && (
            type.GetValueOrDefault() == PileType.Hand
            || type.GetValueOrDefault() == PileType.Play
        );
        if (!shouldModify) return false;
        
        modifiedCost--;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var power = this;
        var card = cardPlay.Card;
        if (cardPlay.Card.Owner.Creature != power.Owner || ShionMechanicsHelper.IsCardNativeToCharacter(card, card.Owner.Character) || AmountThisTurn <= 0)
            return;
        
        PileType? type = card.Pile?.Type;
        bool shouldModify = type.HasValue && (
            type.GetValueOrDefault() == PileType.Hand
            || type.GetValueOrDefault() == PileType.Play
        );
        
        if (!shouldModify) return;
        AmountThisTurn--;
    }
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (Owner == power.Owner && power is StartingGate)
        {
            AdaptAmountAsNeeded((int)amount);
        }
    }
}