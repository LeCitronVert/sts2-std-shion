using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class BattleGloves() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    public const int strengthUp = 1;
    public const int defenseUp = 1;
    public const int energyUp = 1;

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return;
        
        this.Flash();
        
        await GainCardBonus(cardPlay);
    }
    
    private async Task GainCardBonus(CardPlay cardPlay)
    {
        var cardBeingPlayed = cardPlay.Card;
        
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
}