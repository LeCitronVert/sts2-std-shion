using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionRevengeDrive() : ShionDefensiveCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VigorPower>(6),
        new BlockVar(6, ValueProp.Move),
        new PowerVar<RevengeDrive>(4),
        new PowerVar<Guard>(1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars["VigorPower"].BaseValue,
            card.Owner.Creature,
            card
        );
        
        await PowerCmd.Apply<RevengeDrive>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars["RevengeDrive"].BaseValue,
            card.Owner.Creature,
            card
        );

        await CreatureCmd.GainBlock(
            card.Owner.Creature,
            DynamicVars.Block,
            play
        );
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await PowerCmd.Apply<Guard>(
                choiceContext,
                card.Owner.Creature,
                DynamicVars["Guard"].BaseValue,
                card.Owner.Creature,
                card
            );
        }

        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        AddKeyword(CardKeyword.Retain);
        DynamicVars["Guard"].UpgradeValueBy(1);
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