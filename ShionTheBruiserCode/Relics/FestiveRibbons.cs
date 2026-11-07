using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (solo, cursed). Upon pickup, Upgrade ALL your Defends. The next Treasure chest you open is empty
/// (counted like Silver Crucible). Incompatible with Secret Key.
/// </summary>
public class FestiveRibbons : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.SoloOnly;

    public override bool IsCursedOption => true;

    public override bool HasUponPickupEffect => true;

    public override IEnumerable<Type> IncompatibleOptions => [typeof(SecretKey)];

    public override bool IsUsedUp => TreasureRoomsEntered > 0;

    private int _treasureRoomsEntered;

    [SavedProperty]
    public int TreasureRoomsEntered
    {
        get => _treasureRoomsEntered;
        set
        {
            AssertMutable();
            _treasureRoomsEntered = value;
            if (IsUsedUp) Status = RelicStatus.Disabled;
        }
    }

    public override Task AfterObtained()
    {
        var defends = PileType.Deck.GetPile(Owner).Cards.Where(c => c.Tags.Contains(CardTag.Defend) && c.IsUpgradable).ToList();
        if (defends.Count > 0) CardCmd.Upgrade(defends, CardPreviewStyle.HorizontalLayout);
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is TreasureRoom) TreasureRoomsEntered++;
        return Task.CompletedTask;
    }

    // Entering the room counts it first, so the first chest is room 1
    public override bool ShouldGenerateTreasure(Player player) => player != Owner || TreasureRoomsEntered > 1;
}
