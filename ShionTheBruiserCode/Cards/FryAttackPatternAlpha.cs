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
/// Request 2. The Valkyrs deal 9 (12) damage to a random enemy X times.
/// </summary>
public class FryAttackPatternAlpha() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request, FryCardKeywords.Valkyrs];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Request", 2M),
        new DamageVar(9M, ValueProp.Unpowered),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = ResolveEnergyXValue();
        if (x <= 0) return;

        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            for (var i = 0; i < x && CombatState is not null; i++)
            {
                var target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies.ToList());
                if (target is null) break;
                await FryMechanicsHelper.ValkyrsDamage(ctx, [target], DynamicVars.Damage.BaseValue);
            }
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
    }
}
