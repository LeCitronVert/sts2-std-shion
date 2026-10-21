using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of each combat, your Slimy cards gain Replay.
/// </summary>
public class SlimeRibbon : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Shop;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ];

    public override Task BeforeCombatStart()
    {
        foreach (var card in (Owner.PlayerCombatState?.AllCards ?? []).Where(c => SlalMechanicsHelper.IsSlimy(c)))
        {
            card.BaseReplayCount++;
        }

        return Task.CompletedTask;
    }
}
