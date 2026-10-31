using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the end of each combat, enchant a random card in your deck with Glam.
/// </summary>
public class GreyChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Glam>();

    public override Task AfterCombatEnd(CombatRoom room)
    {
        var glam = ModelDb.Enchantment<Glam>();
        var card = Owner.RunState.Rng.Niche.NextItem(PileType.Deck.GetPile(Owner).Cards.Where(glam.CanEnchant));
        if (card is null) return Task.CompletedTask;

        Flash();
        CardCmd.Enchant<Glam>(card, 1m);

        var vfx = NCardEnchantVfx.Create(card);
        if (vfx is not null)
        {
            NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(vfx);
        }
        return Task.CompletedTask;
    }
}
