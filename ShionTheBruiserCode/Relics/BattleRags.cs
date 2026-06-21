using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class BattleRags() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    private bool _wasUsedThisTurn = false;
    public const int strengthUp = 1;
    public const int defenseUp = 1;
    public const int energyUp = 1;

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
            var strengthPower = (await PowerCmd.Apply<ScrappyStrength>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), Owner.Creature, (decimal) strengthUp, Owner.Creature, (CardModel) null!))!;
            return;
        }
        
        if (cardBeingPlayed.Type == CardType.Skill)
        {
            // Gain one temp defense
            var defensePower = await PowerCmd.Apply<ScrappyDexterity>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), Owner.Creature, (decimal) defenseUp, Owner.Creature, (CardModel) null!);
            return;
        }
        
        await PlayerCmd.GainEnergy((decimal) energyUp, Owner);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature))
            return Task.CompletedTask;
        
        _wasUsedThisTurn = false;
        return Task.CompletedTask;
    }

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<BattleGloves>();
}