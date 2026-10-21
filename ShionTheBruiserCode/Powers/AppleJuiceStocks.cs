using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Patches;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X Slimed cards you play from your Hand each turn are played an additional time and grant 1 energy.
/// The additional play doesn't grant energy.
/// </summary>
public class AppleJuiceStocks : SlalTheEndlessPower
{
    public const int EnergyAmount = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.ForEnergy(this),
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner || card is not Slimed) return playCount;

        // Only Slimed played from the Hand count (not auto-played ones, like with Chaotic Clone)
        if (CardAutoPlayTracker.IsBeingAutoPlayed(card)) return playCount;

        var slimedPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted.Count(e =>
            e.Actor == Owner
            && e.CardPlay.IsFirstInSeries
            && !e.CardPlay.IsAutoPlay
            && e.CardPlay.Card is Slimed
            && e.HappenedThisTurn(CombatState));

        return slimedPlayedThisTurn >= Amount ? playCount : playCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card.Owner.Creature != Owner) return;

        Flash();
        await PlayerCmd.GainEnergy(EnergyAmount, Owner.Player!);
    }
}
