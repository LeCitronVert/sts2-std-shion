using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Upon pickup, ALL players Enchant a random basic card of their Deck with Sown.
/// </summary>
public class SouvenirSeed : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Sown>();

    public override Task AfterObtained()
    {
        var sown = ModelDb.Enchantment<Sown>();
        foreach (var player in Owner.RunState.Players)
        {
            var basics = PileType.Deck.GetPile(player).Cards
                .Where(c => c.Rarity == CardRarity.Basic && c.Enchantment == null && sown.CanEnchant(c))
                .ToList();
            if (basics.Count == 0) continue;
            CardCmd.Enchant<Sown>(Owner.RunState.Rng.Niche.NextItem(basics)!, 1m);
        }
        return Task.CompletedTask;
    }
}
