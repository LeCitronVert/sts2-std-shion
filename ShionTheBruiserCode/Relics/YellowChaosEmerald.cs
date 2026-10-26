using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// The first card you play each combat is free. The first card you buy at each merchant visit is free.
/// </summary>
public class YellowChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;

    private bool _boughtCardThisVisit;

    [SavedProperty]
    public bool BoughtCardThisVisit
    {
        get => _boughtCardThisVisit;
        set
        {
            AssertMutable();
            _boughtCardThisVisit = value;
        }
    }

    // ── First card each combat ───────────────────────────────────────────────

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldMakeFree(card)) return false;
        modifiedCost = 0m;
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldMakeFree(card)) return false;
        modifiedCost = 0m;
        return true;
    }

    private bool ShouldMakeFree(CardModel card)
    {
        if (!CombatManager.Instance.IsInProgress) return false;
        if (card.Owner != Owner) return false;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return false;
        return !CombatManager.Instance.History.CardPlaysStarted.Any(e => e.Actor == Owner.Creature);
    }

    // ── First card bought at each merchant ───────────────────────────────────

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
    {
        if (player != Owner || entry is not MerchantCardEntry || BoughtCardThisVisit) return originalPrice;
        return 0m;
    }

    public override Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
    {
        if (player == Owner && itemPurchased is MerchantCardEntry && !BoughtCardThisVisit)
        {
            Flash();
            BoughtCardThisVisit = true;
        }
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is MerchantRoom) BoughtCardThisVisit = false;
        return Task.CompletedTask;
    }
}
