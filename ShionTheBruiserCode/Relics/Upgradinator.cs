using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class Upgradinator : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var relic = this;
        if (!participants.Contains(relic.Owner.Creature) || relic.Owner.PlayerCombatState is { TurnNumber: > 1 })
            return;
        
        relic.Flash();
        
        // Upgrade ALL cards in deck
        foreach (var cardModel in relic.Owner.Deck.Cards)
        {
            if(!cardModel.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
        }
    }
}