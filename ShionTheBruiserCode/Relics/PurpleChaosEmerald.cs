using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class PurpleChaosEmerald() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public override async Task BeforeCombatStart()
    {
        var card = Owner.Creature.CombatState?.CreateCard<EmeraldPowerVision>(Owner);
        
        if (card is null)
        {
            return;
        }
        
        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Draw, 
                Owner,
                CardPilePosition.Random
            )
        );

    }
}