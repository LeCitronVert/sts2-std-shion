using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Turtle Hermit Shell starter. Apply X Weak and Vulnerable. If X is 3 or more, apply X Debilitate.
/// </summary>
public class ShionEnergyBind() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public const int DebilitateThreshold = 3;

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DebilitateThreshold", DebilitateThreshold),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("XAmount").WithMultiplier((card, _) => ((ShionEnergyBind)card).PreviewX()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DebilitatePower>(),
    ];

    private int PreviewX() => CombatState is null ? 0 : PreviewHelper.XValue(this) + (IsUpgraded ? 1 : 0);

    protected override bool ShouldGlowGoldInternal => PreviewX() >= DebilitateThreshold;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        var x = card.ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);
        if (x == 0) return;

        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, x, card.Owner.Creature, card);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, x, card.Owner.Creature, card);
        if (x >= DebilitateThreshold)
        {
            await PowerCmd.Apply<DebilitatePower>(choiceContext, play.Target, x, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
