using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BattleTempo : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    private CardType? _lastUsedThisTurn;
    public const int StrengthUp = 1;
    public const int DefenseUp = 1;
    public const int EnergyUp = 1;
    
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
            return;
        
        await GainCardBonus(cardPlay);
        _lastUsedThisTurn = cardPlay.Card.Type;
    }
    
    private async Task GainCardBonus(CardPlay cardPlay)
    {
        var cardBeingPlayed = cardPlay.Card;

        if (cardBeingPlayed.Type == _lastUsedThisTurn) return;
        
        switch (_lastUsedThisTurn)
        {
            case CardType.Attack:
                // Gain one temp strength
                this.Flash();
                _ = (await PowerCmd.Apply<ScrappyStrength>(new ThrowingPlayerChoiceContext(), Owner, StrengthUp * Amount, Owner, null!))!;
                return;
            case CardType.Skill:
                // Gain one temp defense
                this.Flash();
                await PowerCmd.Apply<ScrappyDexterity>(new ThrowingPlayerChoiceContext(), Owner, DefenseUp * Amount, Owner, null!);
                return;
        }

        if (Owner.Player == null) return;
        this.Flash();
        await PlayerCmd.GainEnergy(EnergyUp * Amount, Owner.Player);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(this.Owner))
            return Task.CompletedTask;

        _lastUsedThisTurn = null;
        return Task.CompletedTask;
    }
}