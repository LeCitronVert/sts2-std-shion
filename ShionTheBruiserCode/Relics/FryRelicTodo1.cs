using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Fry's starter relic. Whenever you end your turn, gain Stars equal to your remaining energy.
/// </summary>
public class FryRelicTodo1 : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected virtual int BonusStars => 0;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature)) return;

        var stars = (Owner.PlayerCombatState?.Energy ?? 0) + BonusStars;
        if (stars <= 0) return;

        Flash();
        await PlayerCmd.GainStars(stars, Owner);
    }

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<FryRelicTodo2>();
}
