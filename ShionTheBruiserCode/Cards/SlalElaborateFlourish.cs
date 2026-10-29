using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 3 (4) Confusion. If you are Weak, apply it again. If the target is Weak, apply it again.
/// </summary>
public class SlalElaborateFlourish() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(3),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ConfusionAmount").WithMultiplier((card, target) =>
            target is null ? 0 : SlalMechanicsHelper.WithMemoryConfusionBonus(card, card.DynamicVars["Confusion"].BaseValue) * TimesApplied(card, target)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.HasPower<WeakPower>();

    private static int TimesApplied(CardModel card, Creature target)
    {
        var times = 1;
        if (card.Owner.Creature.HasPower<WeakPower>()) times++;
        if (target.HasPower<WeakPower>()) times++;
        return times;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var times = TimesApplied(card, play.Target);
        for (var i = 0; i < times && play.Target.IsAlive; i++)
        {
            await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Confusion"].UpgradeValueBy(1);
    }
}
