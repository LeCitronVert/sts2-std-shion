using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

public abstract class CritContributorCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ShionTheBruiserCard(cost, type, rarity, target)
{
    public override async Task BeforeCombatStart()
    {
        if (Owner != null && !Owner.HasPower<ShionCritTrackerPower>())
        {
            await PowerCmd.Apply<ShionCritTrackerPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                amount: 1m,
                applier: Owner.Creature,
                cardSource: null);
        }
    }
}