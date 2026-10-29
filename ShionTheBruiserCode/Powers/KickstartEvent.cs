using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Novel card, gain X energy.
/// Novel is removed when the card is played, so it is checked before the play resolves.
/// </summary>
public class KickstartEvent : SlalOnPowerPlayedPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
    ];

    protected override bool ShouldTrigger(CardModel card) => SlalMechanicsHelper.IsNovel(card);

    protected override async Task OnPowerPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount)
    {
        if (Owner.Player is null) return;
        await PlayerCmd.GainEnergy(amount, Owner.Player);
    }
}
