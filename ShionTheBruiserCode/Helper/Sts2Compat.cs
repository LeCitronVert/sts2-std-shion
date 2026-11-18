using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper
{
    /// <summary>
    /// Bridges the game API differences between the live branch (v0.111.1+) and the beta (v0.113+, built with STS2_BETA).
    /// Call sites are written against the beta API through these wrappers, so the same code compiles on both.
    /// When the beta ships to live: inline these wrappers to the game methods and drop the
    /// <c>#else</c> branches of <c>#if STS2_BETA</c>.
    /// </summary>
    public static class Sts2Compat
    {
        /// <summary>Live takes <c>isFromCard</c>; beta takes the card itself (null when not from a card).</summary>
        public static Task LoseMaxHp(PlayerChoiceContext choiceContext, Creature creature, decimal amount, CardModel? cardSource)
        {
#if STS2_BETA
            return CreatureCmd.LoseMaxHp(choiceContext, creature, amount, cardSource);
#else
            return CreatureCmd.LoseMaxHp(choiceContext, creature, amount, cardSource != null);
#endif
        }

        /// <summary>
        /// Card creation options limited to exactly these (canonical) cards. The game has no custom card list option,
        /// so it's their pools filtered down to them.
        /// </summary>
        public static CardCreationOptions FromCards(IEnumerable<CardModel> cards, CardCreationSource source, CardRarityOddsType rarityOdds)
        {
            var list = cards.ToList();
            var ids = list.Select(c => c.Id).ToHashSet();
            return new CardCreationOptions(list.Select(c => c.Pool).Distinct(), source, rarityOdds, c => ids.Contains(c.Id));
        }

        /// <summary>A random relic off the front of the player's relic list, rarity rolled with their Rewards RNG (live's default).</summary>
        public static RelicModel PullNextRelicForReward(Player player)
        {
#if STS2_BETA
            return RelicFactory.PullNextRelicForRewardFromFront(player);
#else
            return RelicFactory.PullNextRelicFromFront(player);
#endif
        }
    }
}
