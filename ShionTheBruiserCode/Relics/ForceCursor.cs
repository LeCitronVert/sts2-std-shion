using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Treasure chests also contain a card reward (like a regular combat's).
/// Each player's reward is their own, so it works the same in multiplayer.
/// </summary>
public class ForceCursor : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || room is not TreasureRoom) return false;

        rewards.Add(new CardReward(CardCreationOptions.ForRoom(player, RoomType.Monster), 3, Owner));
        Flash();
        return true;
    }
}
