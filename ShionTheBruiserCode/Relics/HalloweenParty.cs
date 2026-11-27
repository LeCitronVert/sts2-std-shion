using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer, cursed). Upon pickup, choose a basic Attack of another player (from their character's starting
/// Deck) to add to your Deck. It gains Eternal. Obtain that player's starting relic. Never offered with Planet Egg.
/// </summary>
public class HalloweenParty : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    public override bool IsCursedOption => true;

    public override bool HasUponPickupEffect => true;

    public override IEnumerable<Type> IncompatibleOptions => [typeof(PlanetEgg)];

    /// <summary>Each other player's basic Attacks, paired with that player.</summary>
    private static List<(Player Player, CardModel Card)> Choices(Player player) => player.RunState.Players
        .Where(p => p != player)
        .SelectMany(p => p.Character.StartingDeck
            .Where(c => c.Rarity == CardRarity.Basic && c.Type == CardType.Attack)
            .DistinctBy(c => c.Id)
            .Select(c => (p, c)))
        .ToList();

    public override bool IsAllowedAtCosmo(Player player) => Choices(player).Count > 0;

    public override async Task AfterObtained()
    {
        var choices = Choices(Owner);
        if (choices.Count == 0) return;

        var cards = choices.Select(c => Owner.RunState.CreateCard(c.Card, Owner)).ToList();
        var chosen = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), cards, Owner, canSkip: false);
        if (chosen == null) return;

        var partner = choices[cards.IndexOf(chosen)].Player;
        PermanentEternal.Add(chosen);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosen, PileType.Deck));

        var relic = partner.Character.StartingRelics.FirstOrDefault();
        if (relic != null) await RelicCmd.Obtain(relic.ToMutable(), Owner);
    }
}
