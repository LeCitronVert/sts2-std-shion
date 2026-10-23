using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever a Token, Status or Curse is played or exhausted, deal X damage to a random enemy.
/// A Token that Exhausts itself when played triggers it twice.
/// </summary>
public class VolatileForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool Triggers(CardModel card) =>
        card.Owner == Owner.Player && (card.Type is CardType.Status or CardType.Curse || card.Rarity == CardRarity.Token);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!Triggers(cardPlay.Card)) return;
        await DealDamage(choiceContext);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (!Triggers(card)) return;
        await DealDamage(choiceContext);
    }

    private async Task DealDamage(PlayerChoiceContext choiceContext)
    {
        if (CombatState is null) return;

        var enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner, null);
    }
}
