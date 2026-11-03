using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Every 4 Attacks you play, gain X Blood.
/// Doesn't stack, like Orbit: each copy played is its own instance with its own counter.
/// </summary>
public class SweatAndTears : SeelsTheRecklessPower
{
    public const int AttacksPerTrigger = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    /// <summary>Attacks left before the next Blood.</summary>
    public override int DisplayAmount => AttacksPerTrigger - GetInternalData<Data>().attacksPlayed % AttacksPerTrigger;

    protected override object InitInternalData() => new Data();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack || !cardPlay.IsFirstInSeries) return;

        var data = GetInternalData<Data>();
        data.attacksPlayed++;
        InvokeDisplayAmountChanged();
        if (data.attacksPlayed % AttacksPerTrigger != 0) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner, Amount);
    }

    private class Data
    {
        public int attacksPlayed;
    }
}
