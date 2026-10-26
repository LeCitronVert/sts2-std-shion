using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class TrainingManual : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    public override async Task BeforeRoomEntered(AbstractRoom room)
    {
        var relic = this;
        if (room.RoomType != RoomType.Elite && room.RoomType != RoomType.Boss)
            return;
        
        relic.Flash();
        CardModel? card = this.Owner.RunState.Rng.Niche.NextItem(PileType.Deck.GetPile(this.Owner).Cards.Where(c => c.IsUpgradable));
        if (card != null)
            CardCmd.Upgrade(card);
    }
}