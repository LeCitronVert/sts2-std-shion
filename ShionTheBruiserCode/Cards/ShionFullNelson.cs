using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFullNelson() : ShionDefensiveCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = play.Card;
        var applier = play.Card.Owner.Creature;
        var target = play.Target;
        ArgumentNullException.ThrowIfNull(target);
        
        await PowerCmd.Apply<FullNelson>(
            choiceContext,
            applier,
            1,
            applier,
            card
        );
        await PowerCmd.Apply<FullNelson>(
            choiceContext,
            target,
            1,
            applier,
            card
        );
        
        await CreatureCmd.GainBlock(applier, DynamicVars.Block, play);
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Block.UpgradeValueBy(3);
    }
    
    // Shion Berserk Stuff
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk)];
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
    }
    
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this) return;
        
        await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(card.Owner.Creature);
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(this.Owner.Creature);
}