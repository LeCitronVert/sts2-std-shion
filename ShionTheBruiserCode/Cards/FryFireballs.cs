using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cooldown 1. Deal 14 (17) damage. Deal 3 (6) damage to ALL other enemies.
/// </summary>
public class FryFireballs() : FryCooldownCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 1;

    protected override int BaseCooldown => 1;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [
        new DamageVar(14M, ValueProp.Move),
        new DynamicVar("OtherDamage", 3M),
    ];

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (CombatState is null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        foreach (var other in CombatState.HittableEnemies.Where(e => e != play.Target).ToList())
        {
            await DamageCmd.Attack(DynamicVars["OtherDamage"].BaseValue)
                .FromCard(this)
                .Targeting(other)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
        DynamicVars["OtherDamage"].UpgradeValueBy(3M);
    }
}
