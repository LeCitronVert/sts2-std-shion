using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 2 Weak. If you are Weak, double (triple) the enemy's Weak. Apply Confusion equal to the enemy's Weak.
/// </summary>
public class SlalSlimeHug(): SlalSlimyCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public const int NormalMultiplier = 2;
    public const int UpgradedMultiplier = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ConfusionAmount").WithMultiplier((card, target) =>
        {
            if (target is null) return 0;
            var weak = target.GetPowerAmount<WeakPower>() + card.DynamicVars.Weak.BaseValue;
            if (card.Owner.Creature.HasPower<WeakPower>())
            {
                weak *= card.IsUpgraded ? UpgradedMultiplier : NormalMultiplier;
            }
            return SlalMechanicsHelper.WithMemoryConfusionBonus(card, weak);
        }),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.Slimy, CardKeyword.Exhaust];

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.HasPower<WeakPower>();

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        // Gunk Up's goop, visual only
        var goop = NGoopyImpactVfx.Create(play.Target);
        if (goop != null) NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(goop);

        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        if (card.Owner.Creature.HasPower<WeakPower>())
        {
            var amountOfWeak = play.Target.GetPowerAmount<WeakPower>();
            if (amountOfWeak > 0)
            {
                var amountToApply = amountOfWeak * (IsUpgraded ? UpgradedMultiplier : NormalMultiplier) - amountOfWeak;
                await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, amountToApply, card.Owner.Creature, card);
            }
        }

        var confusion = play.Target.GetPowerAmount<WeakPower>();
        if (confusion > 0 && play.Target.IsAlive)
        {
            await PowerCmd.Apply<Confusion>(choiceContext, play.Target, confusion, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
