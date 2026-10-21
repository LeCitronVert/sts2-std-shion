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
/// Request X. Deal 8 (10) damage to ALL enemies and gain 4 (5) Strength this turn. Repeat X times.
/// </summary>
public class FryCapeSwish() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8M, ValueProp.Move),
        new PowerVar<StrengthPower>(4M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = ResolveEnergyXValue();
        if (x <= 0) return;

        await FryMechanicsHelper.Request(choiceContext, this, x, async ctx =>
        {
            for (var i = 0; i < x && CombatState is not null; i++)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .TargetingAllOpponents(CombatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(ctx);
                await PowerCmd.Apply<CapeSwishStrength>(ctx, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
            }
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars.Strength.UpgradeValueBy(1M);
    }
}
