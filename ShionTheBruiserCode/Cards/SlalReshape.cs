using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalReshape(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FrailPower>(),
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<Slimed>(),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PowerCmd.Remove<FrailPower>(card.Owner.Creature);
        foreach (CardModel original in PileType.Hand.GetPile(card.Owner).Cards.Where<CardModel>((Func<CardModel, bool>) (c => c != null && c.IsTransformable)).ToList<CardModel>())
        {
            CardModel slime = (CardModel) card.CombatState.CreateCard<Slimed>(card.Owner);
            CardPileAddResult? nullable = await CardCmd.Transform(original, slime);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}