using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Power, gain X Vigor.
/// </summary>
public class GorillaForm : SlalOnPowerPlayedPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override async Task OnPowerPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount)
    {
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, amount, Owner, null);
    }
}
