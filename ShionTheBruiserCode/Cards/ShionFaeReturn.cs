using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

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
        ),
        new CriticalDrawVar(15)
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

        count += CountCardGifts(ownerCreature, history.Entries);

        return count;
    }

    /// <summary>
    /// Times another player gave this creature a card: one per card play that added cards for it
    /// (Pal Armor, Random Portal, Throw Garbage...), plus generated cards credited to another player outside of a card play (Largesse...).
    /// </summary>
    private static int CountCardGifts(Creature ownerCreature, IEnumerable<CombatHistoryEntry> entries)
    {
        int count = 0;
        List<CardPlay> openPlays = [];
        HashSet<CardPlay> giftingPlays = [];

        foreach (var entry in entries)
        {
            switch (entry)
            {
                case CardPlayStartedEntry started:
                    openPlays.Add(started.CardPlay);
                    break;
                case CardPlayFinishedEntry finished:
                    openPlays.Remove(finished.CardPlay);
                    break;
                case CardGeneratedEntry generated when generated.Card.Owner?.Creature == ownerCreature:
                    var play = openPlays.LastOrDefault();
                    if (play != null)
                    {
                        if (play.Card.Owner.Creature != ownerCreature && giftingPlays.Add(play)) count++;
                    }
                    else if (generated.Creator != null && generated.Creator.Creature != ownerCreature)
                    {
                        count++;
                    }
                    break;
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
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
}