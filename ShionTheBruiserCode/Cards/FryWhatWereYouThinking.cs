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
/// Request 5, minus 1 for each 10 Damage Debt you have. The Valkyrs deal 20 damage to ALL enemies. Apply 3 Weak to ALL enemies.
/// </summary>
public class FryWhatWereYouThinking() : FryTheCommanderCard(1, //todo : choose cost
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    public const int DebtPerTurn = 10;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request, FryCardKeywords.Valkyrs];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Request", 5M),
        new DamageVar(20M, ValueProp.Unpowered),
        new PowerVar<WeakPower>(3M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DamageDebt>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var turns = DynamicVars["Request"].IntValue - FryMechanicsHelper.GetDamageDebt(Owner.Creature) / DebtPerTurn;
        await FryMechanicsHelper.Request(choiceContext, this, turns, async ctx =>
        {
            if (CombatState is null) return;
            await FryMechanicsHelper.ValkyrsDamage(ctx, CombatState.HittableEnemies, DynamicVars.Damage.BaseValue);
            await PowerCmd.Apply<WeakPower>(ctx, CombatState.HittableEnemies.ToList(), DynamicVars.Weak.BaseValue, Owner.Creature, this);
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6M); //todo : upgrade not specified
    }
}
