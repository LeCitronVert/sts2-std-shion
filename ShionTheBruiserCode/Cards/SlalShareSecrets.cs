using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 7 Confusion twice. Add a Fresh Idea (+) into your Draw Pile, Hand and Discard Pile.
/// </summary>
public class SlalShareSecrets() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(7),
        new RepeatVar(2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromCard<SlalFreshIdea>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        // Separate applications, so Random increases each of them
        for (var i = 0; i < card.DynamicVars.Repeat.IntValue && play.Target.IsAlive; i++)
        {
            await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);
        }

        var drawResult = await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded), PileType.Draw, card.Owner, CardPilePosition.Random);
        var discardResult = await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded), PileType.Discard, card.Owner);
        await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded), PileType.Hand, card.Owner);
        CardCmd.PreviewCardPileAdd([drawResult, discardResult]);
    }

    protected override void OnUpgrade()
    {
    }
}
