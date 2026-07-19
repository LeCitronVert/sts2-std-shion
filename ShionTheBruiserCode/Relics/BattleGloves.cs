using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class BattleGloves : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    public const int StrengthUp = 1;
    public const int DefenseUp = 1;
    public const int EnergyUp = 1;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Scrappy>(),
    ];

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
}