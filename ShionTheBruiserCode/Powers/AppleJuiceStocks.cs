using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X Slimed you play from your Hand each turn grant 1 energy (auto-played ones don't count).
/// </summary>
public class AppleJuiceStocks : SlalFirstPlaysEachTurnPower
{
    public const int EnergyAmount = 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override bool ShouldTrigger(CardPlay cardPlay) => cardPlay.Card is Slimed && !cardPlay.IsAutoPlay;

    protected override async Task OnTriggered(PlayerChoiceContext choiceContext, CardPlay cardPlay, Player player)
    {
        await PlayerCmd.GainEnergy(EnergyAmount, player);
    }
}
