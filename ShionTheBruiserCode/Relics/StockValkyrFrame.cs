using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// The first card you play each combat is free (energy and Stars). The first card you buy at each merchant is free.
/// </summary>
public class StockValkyrFrame : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool _cardPlayedThisCombat;
    private bool _cardBoughtThisShop;

    public override Task BeforeCombatStart()
    {
        _cardPlayedThisCombat = false;
        Status = RelicStatus.Active;
        return Task.CompletedTask;
    }

    private bool IsFree(CardModel card) =>
        !_cardPlayedThisCombat && card.Owner == Owner && card.Pile?.Type == PileType.Hand;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!IsFree(card)) return false;
        modifiedCost = 0m;
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!IsFree(card)) return false;
        modifiedCost = 0m;
        return true;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_cardPlayedThisCombat || cardPlay.Card.Owner != Owner) return Task.CompletedTask;
        _cardPlayedThisCombat = true;
        Status = RelicStatus.Normal;
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room.RoomType == RoomType.Shop) _cardBoughtThisShop = false;
        return Task.CompletedTask;
    }

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
    {
        if (player != Owner || _cardBoughtThisShop || entry is not MerchantCardEntry) return cost;
        return 0m;
    }

    public override Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
    {
        if (player != Owner || itemPurchased is not MerchantCardEntry || _cardBoughtThisShop) return Task.CompletedTask;
        _cardBoughtThisShop = true;
        Flash();
        return Task.CompletedTask;
    }
}
