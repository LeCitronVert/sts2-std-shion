using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalNextTopic() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SlalFreshIdea>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var ideas = PileType.Exhaust.GetPile(card.Owner).Cards.Where(c => c is SlalFreshIdea).ToList();
        if (ideas.Count == 0) return;

        await CardPileCmd.Add(ideas, PileType.Discard);
        await CreatureCmd.Damage(
            choiceContext,
            card.CombatState.HittableEnemies.ToList(),
            ideas.Count,
            ValueProp.Unblockable | ValueProp.Unpowered,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
