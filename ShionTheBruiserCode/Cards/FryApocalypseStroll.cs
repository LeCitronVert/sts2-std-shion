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
/// Cooldown 1. Deal 20 damage. Deal 12 damage to ALL enemies.
/// </summary>
public class FryApocalypseStroll() : FryCooldownCard(0,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 2;

    protected override int BaseCooldown => 1;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [
        new DamageVar(20M, ValueProp.Move),
        new DynamicVar("AllDamage", 12M),
    ];

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (CombatState is null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);

        await DamageCmd.Attack(DynamicVars["AllDamage"].BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6M); //todo : upgrade not specified
        DynamicVars["AllDamage"].UpgradeValueBy(4M);
    }
}
