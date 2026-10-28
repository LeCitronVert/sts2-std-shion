using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Neow relic (multiplayer, FriendshipBraceletNeowPatch). Choose a Team-Up to add to your Deck:
/// one per class in the party that has a Team-Up with yours, plus one that works with anyone.
/// </summary>
[Pool(typeof(EventRelicPool))]
public class FriendshipBracelet : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool IsAllowedAtNeow(Player player)
    {
        return player.RunState.Players.Count > 1 && TeamUpHelper.GetAvailableTeamUps(player).Any();
    }

    public override async Task AfterObtained()
    {
        var available = TeamUpHelper.GetAvailableTeamUps(Owner).ToList();
        var rng = Owner.PlayerRng.Rewards;

        List<CardModel> choices = [];
        foreach (var partnerClass in Owner.RunState.Players.Where(p => p != Owner).Select(p => p.Character).DistinctBy(c => c.Id))
        {
            var teamUps = available.Where(c => c.PartnerCharacter?.Id == partnerClass.Id).ToList();
            if (teamUps.Count > 0) choices.Add(rng.NextItem(teamUps)!);
        }
        var universal = available.Where(c => c.PartnerCharacter == null).ToList();
        if (universal.Count > 0) choices.Add(rng.NextItem(universal)!);

        var cards = choices.Select(c => Owner.RunState.CreateCard(c, Owner)).ToList();
        if (cards.Count == 0) return;

        var chosen = cards.Count <= 3
            ? await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), cards, Owner, canSkip: true)
            : (await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), cards, Owner,
                new CardSelectorPrefs(new LocString("card_selection", "SHIONTHEBRUISER-TEAM_UP_CHOICE"), 1))).FirstOrDefault();
        if (chosen == null) return;

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosen, PileType.Deck));
    }
}
