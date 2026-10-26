using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Weak and X (X+1) Memory this turn. Retain your Hand this turn.
/// </summary>
public class SlalWaitASecond() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("MemoryAmount").WithMultiplier((card, _) => PreviewHelper.XValue(card) + (card.IsUpgraded ? 1 : 0)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<MemoryThisTurn>(),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var memory = ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);

        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
        await SlalMechanicsHelper.GainMemoryThisTurn(choiceContext, card.Owner.Creature, memory, card);
        await PowerCmd.Apply<RetainHandPower>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
    }
}
