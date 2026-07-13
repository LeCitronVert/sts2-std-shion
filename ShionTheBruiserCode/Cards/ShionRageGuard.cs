using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionRageGuard() : ShionDefensiveCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(3),
        new BlockVar(24, ValueProp.Move),
        new PowerVar<Guard>(1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.Damage(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this
        );
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await PowerCmd.Apply<MightyGuard>(
                choiceContext,
                card.Owner.Creature,
                card.DynamicVars["Guard"].BaseValue,
                card.Owner.Creature,
                card
            );
        }
        else
        {
            await PowerCmd.Apply<Guard>(
                choiceContext,
                card.Owner.Creature,
                card.DynamicVars["Guard"].BaseValue,
                card.Owner.Creature,
                card
            );
        }

        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars["Guard"].UpgradeValueBy(1);
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