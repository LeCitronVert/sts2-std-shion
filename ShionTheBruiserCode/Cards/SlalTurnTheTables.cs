using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 2 Weak. If you are Weak, apply 2 Weak again. Gain 1 Vigor for each Weak on the enemy. Exhaust.
/// </summary>
public class SlalTurnTheTables(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
        new PowerVar<VigorPower>(1),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("VigorAmount").WithMultiplier((card, target) =>
        {
            if (target is null) return 0;
            var repeats = card.Owner.Creature.HasPower<WeakPower>() ? 2 : 1;
            var weakOnTarget = target.GetPowerAmount<WeakPower>() + repeats * card.DynamicVars["WeakPower"].BaseValue;
            return card.DynamicVars["VigorPower"].BaseValue * weakOnTarget;
        }),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.HasPower<WeakPower>();

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        // If you are Weak, apply Weak again
        var repeats = card.Owner.Creature.HasPower<WeakPower>() ? 2 : 1;
        for (var i = 0; i < repeats; i++)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                play.Target,
                DynamicVars["WeakPower"].BaseValue,
                card.Owner.Creature,
                card
            );
        }

        var weakOnTargetAmount = play.Target.GetPowerAmount<WeakPower>();
        if (weakOnTargetAmount > 0)
        {
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                card.Owner.Creature,
                DynamicVars["VigorPower"].BaseValue * weakOnTargetAmount,
                card.Owner.Creature,
                card
            );
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
