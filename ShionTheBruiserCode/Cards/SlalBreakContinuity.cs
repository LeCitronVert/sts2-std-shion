using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalBreakContinuity() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Ancient,
    TargetType.AllEnemies)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DebuffAmount").WithMultiplier((card, _) => PileType.Discard.GetPile(card.Owner).Cards.Count),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        var drawCards = PileType.Draw.GetPile(card.Owner).Cards.ToList();
        var discardCards = PileType.Discard.GetPile(card.Owner).Cards.ToList();

        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, discardCards.Count, card);

        await CardPileCmd.Add(drawCards, PileType.Discard);
        await CardPileCmd.Add(discardCards, PileType.Draw, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
