using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Memory this turn. Gain 1 energy and draw 1 card per Memory you have (permanent and this turn).
/// Memory doesn't play cards this turn. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalProofReading() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MemoryThisTurn>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Counts the Memory gained first
        new CalculatedVar("MemoryAmount").WithMultiplier((card, _) =>
            SlalMechanicsHelper.GetMemory(card.Owner.Creature) + card.DynamicVars["MemoryThisTurn"].IntValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.GainMemoryThisTurn(choiceContext, card.Owner.Creature, card.DynamicVars["MemoryThisTurn"].BaseValue, card);
        await PowerCmd.Apply<MemoryBlocked>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);

        var memory = SlalMechanicsHelper.GetMemory(card.Owner.Creature);
        if (memory <= 0) return;

        await PlayerCmd.GainEnergy(memory, card.Owner);
        await CardPileCmd.Draw(choiceContext, memory, card.Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
