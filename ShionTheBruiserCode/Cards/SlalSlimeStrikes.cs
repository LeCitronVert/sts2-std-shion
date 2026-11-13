using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Slimy. Deal 4 (5) damage. Hit an additional time for each Slimed in your Hand.
/// Its own Slimy Slimed is added before the hits and counts, so it hits at least twice (with room in the Hand).
/// </summary>
public class SlalSlimeStrikes(): SlalSlimyCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override bool SlimesBeforeEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4M, ValueProp.Move),
        new CalculationBaseVar(1),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((card, _) => ((SlalSlimeStrikes)card).PreviewSlimedInHand()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Slimed>()];

    private static int CountSlimed(IEnumerable<CardModel> cards) => cards.Count(c => c is Slimed);

    /// <summary>
    /// Slimed in the Hand once this card's Slimy has added its own (only as many as fit in the Hand).
    /// </summary>
    private int PreviewSlimedInHand()
    {
        if (CombatState is null) return 0;
        var others = PreviewHelper.OtherCardsInHand(this);
        var added = Math.Min(SlimedToAdd(others.Count), Math.Max(0, CardPile.MaxCardsInHand - others.Count));
        return CountSlimed(others) + added;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await AddSlimed();
        var hits = 1 + CountSlimed(PileType.Hand.GetPile(card.Owner).Cards);

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(card, play)
            .Targeting(play.Target)
            // Gunk Up's goop
            .WithHitFx(null, null, "blunt_attack.mp3")
            .WithHitVfxNode(NGoopyImpactVfx.Create)
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
