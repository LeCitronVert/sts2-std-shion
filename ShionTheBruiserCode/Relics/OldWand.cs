using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the end of an Elite fight, enchant a random card in your Deck with Glam.
/// </summary>
public class OldWand : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Glam>();

    public override Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType != RoomType.Elite || Owner.Creature.IsDead) return Task.CompletedTask;

        var glam = ModelDb.Enchantment<Glam>();
        var candidates = Owner.Deck.Cards.Where(c => c.Enchantment == null && glam.CanEnchant(c)).ToList();
        if (candidates.Count == 0) return Task.CompletedTask;

        Flash();
        var card = Owner.RunState.Rng.Niche.NextItem(candidates)!;
        CardCmd.Enchant<Glam>(card, 1m);
        CardCmd.Preview(card);
        return Task.CompletedTask;
    }
}
