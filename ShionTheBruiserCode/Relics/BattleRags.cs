using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class BattleRags : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    private bool _wasUsedThisTurn;
    public const int StrengthUp = 1;
    public const int DefenseUp = 1;
    public const int EnergyUp = 1;

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || _wasUsedThisTurn)
            return;
        
        _wasUsedThisTurn = true;
        this.Flash();
        
        await GainCardBonus(cardPlay);
    }
    
    private async Task GainCardBonus(CardPlay cardPlay)
    {
        var cardBeingPlayed = cardPlay.Card;
        
        _wasUsedThisTurn = true;
        this.Flash();
        
        if (cardBeingPlayed.Type == CardType.Attack)
        {
            // Gain one temp strength
            _ = (await PowerCmd.Apply<ScrappyStrength>(new ThrowingPlayerChoiceContext(), Owner.Creature, StrengthUp, Owner.Creature, null!))!;
            return;
        }
        
        if (cardBeingPlayed.Type == CardType.Skill)
        {
            // Gain one temp defense
            await PowerCmd.Apply<ScrappyDexterity>(new ThrowingPlayerChoiceContext(), Owner.Creature, DefenseUp, Owner.Creature, null!);
            return;
        }
        
        await PlayerCmd.GainEnergy(EnergyUp, Owner);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(this.Owner.Creature))
            return Task.CompletedTask;
        
        _wasUsedThisTurn = false;
        return Task.CompletedTask;
    }

    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<BattleGloves>();
}