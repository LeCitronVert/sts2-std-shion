using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Play ALL your Slimy cards from your Draw Pile, Hand and Discard Pile, and Exhaust them. Exhaust.
/// They are played in the default pile order: Draw Pile, then Hand, then Discard Pile.
/// </summary>
public class SlalTrueSize() : SlalTheEndlessCard(4,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    private static readonly PileType[] SourcePiles = [PileType.Draw, PileType.Hand, PileType.Discard];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var slimyCards = SourcePiles
            .SelectMany(pile => pile.GetPile(card.Owner).Cards)
            .Where(c => c != card && SlalMechanicsHelper.IsSlimy(c) && !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();

        foreach (var slimy in slimyCards)
        {
            if (card.Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding) break;
            // A previous card may have moved it (Exhausted, etc.)
            if (slimy.Pile?.Type is not (PileType.Draw or PileType.Hand or PileType.Discard)) continue;

            slimy.ExhaustOnNextPlay = true;
            await CardPileCmd.Add(slimy, PileType.Play);
            await CardCmd.AutoPlay(choiceContext, slimy, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
