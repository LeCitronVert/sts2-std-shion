using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class GreenChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Green;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<EmeraldDoubleDown>(),
    ];

    public override async Task BeforeCombatStart()
    {
        var card = Owner.Creature.CombatState?.CreateCard<EmeraldDoubleDown>(Owner);

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
