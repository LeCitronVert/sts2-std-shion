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
/// Apply 2X Vulnerable to ALL enemies. Gloat X. Exhaust. (Upgrade: lose Exhaust.)
/// </summary>
public class SeelsFunnyDance() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("VulnerablePerX", 2M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("XAmount").WithMultiplier((card, _) => PreviewHelper.XValue(card)),
        new CalculatedVar("VulnerableAmount").WithMultiplier((card, _) =>
            PreviewHelper.XValue(card) * card.DynamicVars["VulnerablePerX"].IntValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<Gloat>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        var x = ResolveEnergyXValue();
        if (x <= 0) return;

        var creature = Owner.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, x * DynamicVars["VulnerablePerX"].BaseValue, creature, this);
        }
        await SeelsMechanicsHelper.Gloat(choiceContext, creature, x, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
