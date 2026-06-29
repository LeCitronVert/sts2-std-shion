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
        MainFile.Logger.Info($"[CritContributorCard] BeforeCombatStart fired on {GetType().Name}. Owner={(Owner.GetType().Name)}");

        bool alreadyHas = Owner.HasPower<ShionCritTrackerPower>();
        MainFile.Logger.Info($"[CritContributorCard] Owner already has ShionCritTrackerPower: {alreadyHas}");

        if (!alreadyHas)
        {
            MainFile.Logger.Info($"[CritContributorCard] Applying ShionCritTrackerPower to {Owner.Creature.GetType().Name}...");
            await PowerCmd.Apply<ShionCritTrackerPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                amount: 1m,
                applier: Owner.Creature,
                cardSource: null);
            MainFile.Logger.Info($"[CritContributorCard] ShionCritTrackerPower applied.");
        }
    }
}