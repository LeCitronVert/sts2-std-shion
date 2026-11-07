using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Upon pickup, choose 1 of 3 starting cards of other characters to add to your Deck.
/// Basic Strikes and Defends are never offered. Only characters you have unlocked are used.
/// </summary>
public class PlanetEgg : CosmoRelic
{
    public const int Choices = 3;

    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool HasUponPickupEffect => true;

    private static List<CardModel> Pool(Player player) => player.UnlockState.Characters
        .Where(c => c.Id != player.Character.Id)
        .SelectMany(c => c.StartingDeck)
        .Where(c => !c.IsBasicStrikeOrDefend)
        .DistinctBy(c => c.Id)
        .OrderBy(c => c.Id)
        .ToList();

    public override bool IsAllowedAtCosmo(Player player) => Pool(player).Count > 0;

    public override async Task AfterObtained()
    {
        var pool = Pool(Owner);
        var rng = Owner.PlayerRng.Rewards;
        List<CardModel> choices = [];
        while (choices.Count < Choices && pool.Count > 0)
        {
            var card = rng.NextItem(pool)!;
            pool.Remove(card);
            choices.Add(Owner.RunState.CreateCard(card, Owner));
        }
        if (choices.Count == 0) return;

        var chosen = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), choices, Owner, canSkip: false);
        if (chosen != null) CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosen, PileType.Deck));
    }
}
