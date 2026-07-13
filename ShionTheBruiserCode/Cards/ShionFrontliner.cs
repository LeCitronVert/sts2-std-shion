using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFrontliner() : ShionDefensiveCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllAllies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(20, ValueProp.Move)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var applier = card.Owner.Creature;
        
        foreach (Creature creature in card.CombatState?.GetTeammatesOf(card.Owner.Creature).Where(c => c?.IsPlayer ?? false)!)
        {
            if (creature.Player == card.Owner)
            {
                await PowerCmd.Apply<Frontliner>(
                    choiceContext,
                    creature,
                    1,
                    applier,
                    card
                );
            }
            else
            {
                await PowerCmd.Apply<IntangiblePower>(
                    choiceContext,
                    creature,
                    1,
                    applier,
                    card
                );
            }
        }
        
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await CreatureCmd.GainBlock(
                card.Owner.Creature,
                DynamicVars.Block,
                play
            );
        }
        
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Block.UpgradeValueBy(10);
    }
    
    public override Task BeforeCombatStart()    
    {
        if (!Owner.Creature.HasPower<ShionCritTrackerPower>())
        {
            ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
        }
        
        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return Task.CompletedTask;
        
        ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(card.Owner.Creature);
        
        return Task.CompletedTask;
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature);
}