using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalSeeWhatSticks(): SlalSlimyCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(IsUpgraded),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;

        if (card.CombatState is null) return;
         
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        CardPileAddResult addResult = await CardPileCmd.AddGeneratedCardToCombat((CardModel) card.CombatState.CreateCard<SlalFreshIdea>(card.Owner), PileType.Hand, card.Owner);
        var cardAdded = addResult.cardAdded;

        if (IsUpgraded)
        {
            CardCmd.Upgrade(cardAdded);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
