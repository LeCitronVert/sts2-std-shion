using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class PurpleChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Purple;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<EmeraldPowerVision>(),
        HoverTipFactory.FromKeyword(DreamersKeywords.EmeraldPower)
    ];

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
