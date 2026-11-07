using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Ancients;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer, cursed). Obtain the Chaos Emerald of another party member's character, and add 2 of that
/// character's basic Defends to your Deck.
/// Only offered when you unlocked (in your save) the Emerald of at least one partner's character, and don't hold it yet.
/// Unlocks are read from the synced unlock state (<see cref="CosmoUnlockExtras"/>), so every client agrees.
/// </summary>
public class ChaosRing : CosmoRelic
{
    public const int Defends = 2;

    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    public override bool IsCursedOption => true;

    public override bool HasUponPickupEffect => true;

    private static IEnumerable<Player> EligiblePartners(Player player)
    {
        var unlocked = CosmoUnlockExtras.Of(player);
        return player.RunState.Players.Where(p =>
        {
            if (p == player) return false;
            var color = ChaosEmeraldHelper.ColorFor(p);
            return unlocked.HasEmerald(color) && !player.Relics.Any(r => r is ChaosEmeraldRelic emerald && emerald.Color == color);
        });
    }

    public override bool IsAllowedAtCosmo(Player player) => EligiblePartners(player).Any();

    public override async Task AfterObtained()
    {
        var partners = EligiblePartners(Owner).ToList();
        if (partners.Count == 0) return;
        var partner = Owner.PlayerRng.Rewards.NextItem(partners)!;

        await RelicCmd.Obtain(ChaosEmeraldHelper.SoloRelic(ChaosEmeraldHelper.ColorFor(partner)).ToMutable(), Owner);

        var defend = partner.Character.StartingDeck.FirstOrDefault(c => c.Rarity == CardRarity.Basic && c.Tags.Contains(CardTag.Defend));
        if (defend == null) return;
        for (var i = 0; i < Defends; i++)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Owner.RunState.CreateCard(defend, Owner), PileType.Deck));
        }
    }
}
