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

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Request 3. ALL enemies lose 5 Strength this turn. The Valkyrs deal 33 damage to ALL enemies.
/// </summary>
public class FryTodo16() : FryTheCommanderCard(2, //todo : choose cost
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request, FryCardKeywords.Valkyrs];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Request", 3M),
        new PowerVar<StrengthPower>(5M),
        new DamageVar(33M, ValueProp.Unpowered),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            if (CombatState is null) return;
            await PowerCmd.Apply<FryTodo16StrengthDown>(ctx, CombatState.HittableEnemies.ToList(), DynamicVars.Strength.BaseValue, Owner.Creature, this);
            await FryMechanicsHelper.ValkyrsDamage(ctx, CombatState.HittableEnemies, DynamicVars.Damage.BaseValue);
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(7M); //todo : upgrade not specified
    }
}
