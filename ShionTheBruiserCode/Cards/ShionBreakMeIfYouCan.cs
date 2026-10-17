using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBreakMeIfYouCan() : ShionDefensiveCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int GuardInitialAmount = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GuardAmount", BreakMeIfYouCan.GuardAmount),
        new DynamicVar("GuardInitialAmount", GuardInitialAmount),
        new DynamicVar("VigorAmount", BreakMeIfYouCan.VigorAmount),
        new PatchUpVar(3)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, ShionCardKeywords.Defensive];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<Guard>(choiceContext, card.Owner.Creature, GuardInitialAmount, card.Owner.Creature, card);
        await PowerCmd.Apply<BreakMeIfYouCan>(choiceContext, card.Owner.Creature, 1M, card.Owner.Creature, card);
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
    
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