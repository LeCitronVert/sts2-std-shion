using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFaeReturn() : ShionTheBruiserCard(3,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(24M),
        new ExtraDamageVar(10M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
            (card, target) => (Decimal) CountAllyBeneficialEffects(card)
        )
    ];
    
    public override CardMultiplayerConstraint MultiplayerConstraint => 
        CardMultiplayerConstraint.MultiplayerOnly;
    
    private static int CountAllyBeneficialEffects(CardModel card)
    {
        if (card.CombatState == null) return 0;
        
        var ownerCreature = card.Owner.Creature;
        var history = CombatManager.Instance.History;
        int count = 0;

        var powerEntries = history.Entries.OfType<PowerReceivedEntry>();
        foreach (var entry in powerEntries)
        {
            if (entry.Power.Owner == ownerCreature &&
                entry.Applier != null &&
                entry.Applier != ownerCreature &&
                entry.Applier.Side == CombatSide.Player &&
                entry.Power.Type == PowerType.Buff &&
                entry.Amount > 0)
            {
                count++;
            }
        }

        var blockEntries = history.Entries.OfType<BlockGainedEntry>();
        foreach (var entry in blockEntries)
        {
            if (entry.Receiver == ownerCreature &&
                entry.CardPlay != null &&
                entry.CardPlay.Card.Owner.Creature != ownerCreature &&
                entry.CardPlay.Card.Owner.Creature.Side == CombatSide.Player &&
                entry.Amount > 0)
            {
                count++;
            }
        }

        return count;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2M);
        DynamicVars.ExtraDamage.UpgradeValueBy(2M);
    }
}