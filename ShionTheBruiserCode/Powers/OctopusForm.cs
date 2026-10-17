using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times you target an enemy each turn (with an Attack or a Skill), that enemy loses 1 Strength this turn.
/// Random targets count. When several enemies are targeted at once, the left-most ones are affected first.
/// </summary>
public class OctopusForm : SlalTheEndlessPower
{
    private class Data
    {
        public int procsThisTurn;
        public CardModel? currentCard;
        public readonly List<Creature> affectedByCurrentCard = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override object InitInternalData() => new Data();

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner)) GetInternalData<Data>().procsThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type is not (CardType.Attack or CardType.Skill))
            return Task.CompletedTask;

        var data = GetInternalData<Data>();
        data.currentCard = cardPlay.Card;
        data.affectedByCurrentCard.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        TrackAffectedEnemy(target, cardSource);
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (applier == Owner) TrackAffectedEnemy(power.Owner, cardSource);
        return Task.CompletedTask;
    }

    private void TrackAffectedEnemy(Creature target, CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (cardSource == null || cardSource != data.currentCard) return;
        if (target.Side == Owner.Side || data.affectedByCurrentCard.Contains(target)) return;
        data.affectedByCurrentCard.Add(target);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var data = GetInternalData<Data>();
        if (cardPlay.Card != data.currentCard || CombatState is null) return;
        data.currentCard = null;

        List<Creature> targets = cardPlay.Card.TargetType switch
        {
            TargetType.AnyEnemy when cardPlay.Target != null => [cardPlay.Target],
            TargetType.AllEnemies => CombatState.HittableEnemies.ToList(),
            TargetType.RandomEnemy => data.affectedByCurrentCard.ToList(),
            _ => [],
        };

        foreach (var enemy in targets.Where(e => e.IsAlive))
        {
            if (data.procsThisTurn >= Amount) break;
            data.procsThisTurn++;

            Flash();
            await PowerCmd.Apply<OctopusFormStrengthDown>(choiceContext, enemy, 1, Owner, null);
        }
    }
}
