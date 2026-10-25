using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// This turn, whenever ANYONE plays a card, apply Amount Confusion to a random enemy, then Amount goes up by the "Increase" var.
/// </summary>
public class RandomBlast : SlalTheEndlessPower
{
    public const int BaseIncrease = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Increase", BaseIncrease)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    private class Data
    {
        /// <summary>The Random Blast play that applied this power doesn't count.</summary>
        public readonly HashSet<CardModel> playsToSkip = [];
    }

    protected override object InitInternalData() => new Data();

    public void SkipPlayOf(CardModel card)
    {
        GetInternalData<Data>().playsToSkip.Add(card);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (GetInternalData<Data>().playsToSkip.Remove(cardPlay.Card) || CombatState is null || Owner.Player is null) return;

        var enemy = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return;

        Flash();
        await PowerCmd.Apply<Confusion>(choiceContext, enemy, Amount, Owner, null);
        await PowerCmd.Apply<RandomBlast>(choiceContext, Owner, DynamicVars["Increase"].BaseValue, Owner, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
