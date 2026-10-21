using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of each Elite or Boss fight, procure a Blood Potion.
/// </summary>
public class CrystalGarland : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<BloodPotion>()];

    public override async Task BeforeCombatStart()
    {
        var roomType = Owner.RunState.CurrentRoom?.RoomType;
        if (roomType is not (RoomType.Elite or RoomType.Boss)) return;

        Flash();
        await PotionCmd.TryToProcure<BloodPotion>(Owner);
    }
}
