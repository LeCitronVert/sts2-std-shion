using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 10 (14) Block, plus 3 (4) Block for each Slimed in your Hand, then play those Slimed.
/// Ignore your Vulnerable until your next turn.
/// </summary>
public class SlalFavouriteBucket(): SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(10),
        new CalculationExtraVar(3),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) => GetSlimedInHand(card).Count),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<IgnoreVulnerable>(),
    ];

    private static List<CardModel> GetSlimedInHand(CardModel card) =>
        PileType.Hand.GetPile(card.Owner).Cards.Where(c => c is Slimed).ToList();

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.CalculatedBlock.Calculate(play.Target), card.DynamicVars.CalculatedBlock.Props, play);

        foreach (var slimed in GetSlimedInHand(card))
        {
            if (CombatManager.Instance.IsOverOrEnding) break;
            // A previous Slimed may have moved it
            if (slimed.Pile?.Type != PileType.Hand) continue;
            await CardCmd.AutoPlay(choiceContext, slimed, null);
        }

        await PowerCmd.Apply<IgnoreVulnerable>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4);
        DynamicVars.CalculationExtra.UpgradeValueBy(1);
    }
}
