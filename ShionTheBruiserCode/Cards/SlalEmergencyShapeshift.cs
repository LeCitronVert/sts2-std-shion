using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Transform 3 random cards in your Hand into Sword Form, Octopus Form and Box of Nails Form, in this order.
/// Doesn't create more Forms than there are cards in Hand. Playable Novel cards are picked last.
/// </summary>
public class SlalEmergencyShapeshift() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<SlalSwordForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalOctopusForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalBoxOfNailsForm>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var rng = card.Owner.RunState.Rng.CombatCardSelection;
        var hand = PileType.Hand.GetPile(card.Owner).Cards.Where(c => c.IsTransformable).ToList();

        bool IsPlayableNovel(CardModel c) =>
            c.Keywords.Contains(SlalCardKeywords.Novel) && !c.Keywords.Contains(CardKeyword.Unplayable);

        var targets = hand.Where(c => !IsPlayableNovel(c)).TakeRandom(3, rng)
            .Concat(hand.Where(IsPlayableNovel).TakeRandom(3, rng))
            .Take(3)
            .ToList();

        List<CardModel> forms = [
            SlalMechanicsHelper.CreateCard<SlalSwordForm>(card.Owner, IsUpgraded),
            SlalMechanicsHelper.CreateCard<SlalOctopusForm>(card.Owner, IsUpgraded),
            SlalMechanicsHelper.CreateCard<SlalBoxOfNailsForm>(card.Owner, IsUpgraded),
        ];

        for (var i = 0; i < targets.Count; i++)
        {
            await CardCmd.Transform(targets[i], forms[i]);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
