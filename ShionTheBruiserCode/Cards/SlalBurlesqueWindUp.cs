using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Weak. Draw as many cards as your Weak. This turn, whenever you play a card, gain 1 Vigor. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalBurlesqueWindUp() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BurlesqueWindUp>(1),
        new PowerVar<WeakPower>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Counts the Weak gained first
        new CalculatedVar("DrawAmount").WithMultiplier((card, _) =>
            card.Owner.Creature.GetPowerAmount<WeakPower>() + card.DynamicVars.Weak.BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        var weak = card.Owner.Creature.GetPowerAmount<WeakPower>();
        if (weak > 0)
        {
            await CardPileCmd.Draw(choiceContext, weak, card.Owner);
        }

        await PowerCmd.Apply<BurlesqueWindUp>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars["BurlesqueWindUp"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
