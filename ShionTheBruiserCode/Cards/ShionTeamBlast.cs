using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionTeamBlast() : ShionTheBruiserCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override bool GainsBlock => true;

    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(30, ValueProp.Move),
        new CritDisplayTooltipVar(),
        new DynamicVar("CritHitRate", 20),
        new DynamicVar("CritAttackRate", 10),
        new PowerVar<RegenPower>(0)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        DreamersKeywords.TeamBlast
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var applier = card.Owner.Creature;
        
        foreach (Creature target in card.CombatState?.GetTeammatesOf(card.Owner.Creature).Where(c => c?.IsPlayer ?? false)!)
        {
            await CreatureCmd.GainBlock(
                target, 
                card.DynamicVars.Block, 
                play
            );
            
            await ShionMechanicsHelper.EnsureCreatureHasCritTracker(target);
            await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(target);
            
            await PowerCmd.Apply<DragonCheer>(
                choiceContext,
                target,
                1,
                applier,
                card
            );
            
            var regenAmount = DynamicVars["RegenPower"].BaseValue;
            if (regenAmount > 0)
            {
                await PowerCmd.Apply<RegenPower>(
                    choiceContext,
                    target,
                    regenAmount,
                    applier,
                    card
                );
            }
        }
        
        await DreamersMechanicsHelper.ExhaustAllTeamBlasts(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RegenPower"].UpgradeValueBy(5);
        HoverTips.AddItem(HoverTipFactory.FromPower<RegenPower>());
    }
}