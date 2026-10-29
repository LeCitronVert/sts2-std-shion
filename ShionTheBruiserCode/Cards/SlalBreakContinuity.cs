using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Memory. This combat, Memory doesn't play cards.
/// Instead, Memory gives 1 energy and draws 1 card for the first X cards played each turn. Exhaust (upgrade: no Exhaust).
/// </summary>
//todo : type not specified
public class SlalBreakContinuity() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Ancient,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Memory>(1),
        new PowerVar<BreakContinuity>(1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.GainMemory(choiceContext, card.Owner.Creature, card.DynamicVars["Memory"].BaseValue, card);
        await PowerCmd.Apply<BreakContinuity>(choiceContext, card.Owner.Creature, card.DynamicVars["BreakContinuity"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
