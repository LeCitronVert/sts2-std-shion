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
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionRageGuard() : ShionDefensiveCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(3),
        new BlockVar(12, ValueProp.Move),
        new PowerVar<Guard>(1),
        new PatchUpVar(3)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await ShionMechanicsHelper.PatchUpCreature(card.DynamicVars[PatchUpVar.Key].IntValue, card.Owner.Creature);
        }
        else
        {
            await CreatureCmd.Damage(
                choiceContext,
                card.Owner.Creature,
                card.DynamicVars.HpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this
            );
        }
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        await PowerCmd.Apply<Guard>(choiceContext, card.Owner.Creature, card.DynamicVars["Guard"].IntValue, card.Owner.Creature, this);

        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars["Guard"].UpgradeValueBy(1);
    }
    
    public override async Task BeforeCombatStart()    
    {
        if (!Owner.Creature.HasPower<ShionCritTrackerPower>())
        {
            await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return;
        
        await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(card.Owner.Creature);
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature);
}