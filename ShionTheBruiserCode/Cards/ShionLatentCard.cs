using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public abstract class ShionLatentCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ShionTheBruiserCard(cost, type, rarity, target)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Latent];
    
    public override void ModifyShuffleOrder(
        Player player,
        List<CardModel> cards,
        bool isInitialShuffle)
    {
        if (!isInitialShuffle || player != Owner) return;
        
        cards.Where(c => c == this).ToList().ForEach(c =>
        {
            cards.Remove(c);
            cards.Add(c);
        });
    }
}