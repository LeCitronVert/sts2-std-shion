using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalDraftPhase(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(this.IsUpgraded)
    ];


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        CardPileAddResult combat = await CardPileCmd.AddGeneratedCardToCombat((CardModel) card.CombatState.CreateCard<SlalFreshIdea>(card.Owner), PileType.Hand, card.Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(combat.cardAdded);
        }
        
        await PowerCmd.Apply<EnergyNextTurnPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars.Energy.BaseValue,
            Owner.Creature,
            null!
        );
    }

    protected override void OnUpgrade()
    {
        
    }
}