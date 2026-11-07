using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Upon pickup, procure a random potion. When you enter a Boss room, gain 1 potion slot (for the rest of the run)
/// and procure a random potion.
/// </summary>
public class FlosFridge : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool HasUponPickupEffect => true;

    private async Task ProcureRandomPotion()
    {
        var potion = PotionFactory.CreateRandomPotionOutOfCombat(Owner, Owner.PlayerRng.Rewards).ToMutable();
        await PotionCmd.TryToProcure(potion, Owner);
    }

    public override async Task AfterObtained() => await ProcureRandomPotion();

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room.RoomType != RoomType.Boss) return;

        Flash();
        await PlayerCmd.GainMaxPotionCount(1, Owner);
        await ProcureRandomPotion();
    }
}
