using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you play a Power, deal damage to ALL enemies equal to the number of Powers you played this combat.
/// </summary>
public class WhiteRibbon : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    public override int DisplayAmount => PowersPlayedThisCombat;

    private int _powersPlayedThisCombat;
    private int PowersPlayedThisCombat
    {
        get => _powersPlayedThisCombat;
        set
        {
            AssertMutable();
            _powersPlayedThisCombat = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override Task BeforeCombatStart()
    {
        PowersPlayedThisCombat = 0;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        PowersPlayedThisCombat = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Power || Owner.Creature.CombatState is null) return;

        PowersPlayedThisCombat++;
        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature.CombatState.HittableEnemies,
            PowersPlayedThisCombat,
            ValueProp.Unpowered,
            Owner.Creature
        );
    }
}
