using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Maim 2. ALL enemies lose 2 HP. Gain 4X Tainted. (Retain.) Multiplayer only.
/// </summary>
public class SeelsBigTargetSign() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Maim];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        MaimHelper.Var(2M),
        new DynamicVar("EnemyHpLoss", 2M),
        new DynamicVar("TaintedPerX", 4M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("TaintedAmount").WithMultiplier((card, _) =>
            PreviewHelper.XValue(card) * card.DynamicVars["TaintedPerX"].BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<TaintedPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        var x = ResolveEnergyXValue();

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            await Sts2Compat.Damage(choiceContext, enemies, DynamicVars["EnemyHpLoss"].BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature, this, play);
        }

        if (x > 0)
        {
            await PowerCmd.Apply<TaintedPower>(choiceContext, Owner.Creature, x * DynamicVars["TaintedPerX"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
