using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// If the enemy intends to attack, gain Bad Faith equal to its full attack power. Exhaust.
/// </summary>
public class FryTodo20() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("BadFaithAmount").WithMultiplier((card, target) => target is null ? 0 : GetAttackPower(card, target)),
    ];

    private static decimal GetAttackPower(CardModel card, Creature target)
    {
        var monster = target.Monster;
        if (monster is null || !monster.IntendsToAttack) return 0;

        return monster.NextMove.Intents
            .OfType<AttackIntent>()
            .Sum(i => i.GetTotalDamage([card.Owner.Creature], target));
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BadFaith>()];

    protected override bool ShouldGlowGoldInternal => CombatState?.HittableEnemies.Any(FryMechanicsHelper.IntendsToAttack) ?? false;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var power = GetAttackPower(this, play.Target);
        await FryMechanicsHelper.GainBadFaith(choiceContext, Owner.Creature, power, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
