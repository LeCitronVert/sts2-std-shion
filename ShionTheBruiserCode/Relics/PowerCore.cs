using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo relic (multiplayer, cursed). Choose 1 of 3 Team Blasts to add to your Deck:
/// your character's own Team Blast if it has one, the rest generic (Colorless) ones. Then add a random Curse to your Deck.
/// </summary>
public class PowerCore : CosmoRelic
{
    public const int Choices = 3;

    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    public override bool IsCursedOption => true;

    public override bool IsAllowedAtCosmo(Player player) => GetGenericTeamBlasts().Any();

    private static IEnumerable<CardModel> GetGenericTeamBlasts() =>
        ModelDb.CardPool<ColorlessCardPool>().AllCards.Where(DreamersMechanicsHelper.IsTeamBlast);

    private static IEnumerable<CardModel> GetOwnTeamBlasts(Player player) =>
        player.Character.CardPool.AllCards.Where(DreamersMechanicsHelper.IsTeamBlast);

    public override async Task AfterObtained()
    {
        var rng = Owner.PlayerRng.Rewards;

        List<CardModel> choices = [];
        var own = GetOwnTeamBlasts(Owner).OrderBy(c => c.Id).ToList();
        if (own.Count > 0) choices.Add(rng.NextItem(own)!);

        var generic = GetGenericTeamBlasts().OrderBy(c => c.Id).ToList();
        while (choices.Count < Choices && generic.Count > 0)
        {
            var card = rng.NextItem(generic)!;
            generic.Remove(card);
            choices.Add(card);
        }

        var cards = choices.Select(c => Owner.RunState.CreateCard(c, Owner)).ToList();
        if (cards.Count > 0)
        {
            var chosen = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), cards, Owner, canSkip: false);
            if (chosen != null)
            {
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosen, PileType.Deck));
            }
        }

        await DreamersMechanicsHelper.AddRandomCurseToDeck(Owner);
    }
}
