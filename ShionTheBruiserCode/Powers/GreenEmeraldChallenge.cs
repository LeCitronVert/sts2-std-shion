using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Green challenge (on each player): whenever you play a card, lose 1 Strength and 1 Dexterity until next turn.
/// </summary>
public class GreenEmeraldChallenge : EmeraldChallengePower
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Green;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;

        Flash();
        await PowerCmd.Apply<GreenChallengeStrengthDown>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Apply<GreenChallengeDexterityDown>(choiceContext, Owner, Amount, Owner, null);
    }
}
