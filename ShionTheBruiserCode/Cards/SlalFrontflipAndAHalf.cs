using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 1 Weak to ALL enemies and yourself. Deal 5 (7) damage to ALL enemies. Take 5 damage.
/// If that damage made you lose HP, ALL enemies gain 10 (15) Confusion.
/// </summary>
public class SlalFrontflipAndAHalf() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5M, ValueProp.Move),
        new PowerVar<WeakPower>(1),
        new DynamicVar("SelfDamage", 5M),
        new PowerVar<Confusion>(10),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        await SlalMechanicsHelper.ApplyToAllEnemies<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card);
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        // Blockable, so Block can prevent the Confusion
        var selfDamage = await CreatureCmd.Damage(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["SelfDamage"].BaseValue,
            ValueProp.Unpowered,
            card.Owner.Creature,
            card
        );

        if (selfDamage.Any(r => r.UnblockedDamage > 0))
        {
            await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, card.DynamicVars["Confusion"].BaseValue, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars["Confusion"].UpgradeValueBy(5);
    }
}
