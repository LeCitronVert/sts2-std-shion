using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public abstract class ShionDefensiveCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ShionTheBruiserCard(cost, type, rarity, target)
{
    protected async Task ApplyDefensiveToPlayer()
    {
        await PowerCmd.Apply<DefensiveDebuff>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            amount: 1,
            applier: Owner.Creature,
            cardSource: this
        );
    }

    // Defensive cards are only playable if the player didn't play any attacks this turn
    protected override bool IsPlayable
    {
        get
        {
            var amount = CombatManager.Instance.History.Entries
                .OfType<CardPlayStartedEntry>()
                .Count((Func<CardPlayStartedEntry, bool>) (e => e.HappenedThisTurn(CombatState) && e.Actor == this.Owner.Creature && e.CardPlay.Card.Type == CardType.Attack))
            ;
            
            return amount == 0;
        }
    }
}