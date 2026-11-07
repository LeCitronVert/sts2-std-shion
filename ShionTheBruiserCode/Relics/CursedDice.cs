using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Event relic, any character. Obtained from "The Gambler" event (Events/TheGambler.cs, "Play until you win").
/// Upon pickup, add Gambling Addiction to your Deck. Your combat card rewards gain an additional Gamble card
/// (a Gamble-tagged card from Seels' pool, whoever holds it). Reward hook modelled on Lasting Candy.
/// </summary>
[Pool(typeof(EventRelicPool))]
public class CursedDice : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsGamblingAddiction>(),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble),
    ];

    public override async Task AfterObtained()
    {
        var card = Owner.RunState.CreateCard(ModelDb.Card<SeelsGamblingAddiction>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }

    private static bool IsGambleCard(CardModel card) =>
        card.Tags.Contains(SeelsCardTags.Gamble)
        && card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare;

    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
    {
        if (Owner != player || creationOptions.Source != CardCreationSource.Encounter) return false;

        var gambleCards = new CardCreationOptions([ModelDb.CardPool<SeelsTheRecklessCardPool>()],
                CardCreationSource.Other, creationOptions.RarityOdds, IsGambleCard)
            .GetPossibleCards(player).ToList();

        // Prefer one that isn't already in the reward.
        var fresh = gambleCards.Where(c => options.TrueForAll(o => o.originalCard.Id != c.Id)).ToList();
        if (fresh.Count > 0) gambleCards = fresh;
        if (gambleCards.Count == 0) return false;

        var rewardOptions = new CardCreationOptions(gambleCards, CardCreationSource.Other, creationOptions.RarityOdds)
            .WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);
        var card = CardFactory.CreateForReward(Owner, 1, rewardOptions).FirstOrDefault()?.Card;
        if (card == null) return false;

        var result = new CardCreationResult(card);
        result.ModifyCard(card, this);
        options.Add(result);
        return true;
    }
}
