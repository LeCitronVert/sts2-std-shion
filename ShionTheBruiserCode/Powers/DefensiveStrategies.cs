using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Defensive card, your Guard and Mighty Guard last X more turns.
/// </summary>
public class DefensiveStrategies : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Defensive),
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromPower<MightyGuard>(),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (
            cardPlay.Card.Owner.Creature != Owner
            || !cardPlay.Card.Keywords.Contains(ShionCardKeywords.Defensive)
        ) return;

        var power = this;
        var extended = false;
        if (power.Owner.HasPower<Guard>())
        {
            await PowerCmd.Apply<Guard>(choiceContext, power.Owner, Amount, power.Owner, null);
            extended = true;
        }
        if (power.Owner.HasPower<MightyGuard>())
        {
            await PowerCmd.Apply<MightyGuard>(choiceContext, power.Owner, Amount, power.Owner, null);
            extended = true;
        }
        if (extended) Flash();
    }
}
