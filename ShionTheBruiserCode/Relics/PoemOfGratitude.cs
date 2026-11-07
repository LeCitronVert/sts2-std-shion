using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer, cursed). Upon pickup, ALL players obtain a random relic.
/// Your first Elite combat doesn't give you a relic.
/// </summary>
public class PoemOfGratitude : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    public override bool IsCursedOption => true;

    public override bool HasUponPickupEffect => true;

    private bool _eliteRelicSkipped;

    public override bool IsUsedUp => _eliteRelicSkipped;

    [SavedProperty]
    public bool EliteRelicSkipped
    {
        get => _eliteRelicSkipped;
        set
        {
            AssertMutable();
            _eliteRelicSkipped = value;
            Status = value ? RelicStatus.Disabled : RelicStatus.Active;
        }
    }

    public override async Task AfterObtained()
    {
        Status = RelicStatus.Active;
        foreach (var player in Owner.RunState.Players)
        {
            await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(player).ToMutable(), player);
        }
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || EliteRelicSkipped || room?.RoomType != RoomType.Elite) return false;

        Flash();
        EliteRelicSkipped = true;
        return rewards.RemoveAll(r => r is RelicReward) > 0;
    }
}
