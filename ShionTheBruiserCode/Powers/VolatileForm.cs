using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever a Token or Status card is either played or Exhausted, deal X damage to a random enemy.
/// A card that Exhausts itself when played only triggers it once (for the play, not for the Exhaust).
/// </summary>
public class VolatileForm : SlalTheEndlessPower
{
    private class Data
    {
        /// <summary>Cards whose last play sends them to the Exhaust Pile: that Exhaust doesn't trigger again.</summary>
        public readonly HashSet<CardModel> exhaustingFromPlay = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    private bool Triggers(CardModel card) =>
        card.Owner == Owner.Player && (card.Type == CardType.Status || card.Rarity == CardRarity.Token);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!Triggers(cardPlay.Card)) return;
        // The card is moved to its result pile after its last play (and after this hook)
        if (cardPlay.ResultPile == PileType.Exhaust && cardPlay.PlayIndex == cardPlay.PlayCount - 1)
        {
            GetInternalData<Data>().exhaustingFromPlay.Add(cardPlay.Card);
        }
        await DealDamage(choiceContext);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (GetInternalData<Data>().exhaustingFromPlay.Remove(card)) return;
        if (!Triggers(card)) return;
        await DealDamage(choiceContext);
    }

    public override Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // In case a play was interrupted before its card reached the Exhaust Pile
        GetInternalData<Data>().exhaustingFromPlay.Clear();
        return Task.CompletedTask;
    }

    private async Task DealDamage(PlayerChoiceContext choiceContext)
    {
        if (CombatState is null) return;

        var enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return;

        Flash();
        await Sts2Compat.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner, null, null);
    }
}
