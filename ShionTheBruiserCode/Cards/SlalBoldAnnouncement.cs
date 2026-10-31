using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 6 Confusion and deal 1 damage to ALL enemies (Confusion first, so the hit can execute). Gain 1 Flashback for each enemy.
/// </summary>
public class SlalBoldAnnouncement() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1M, ValueProp.Move),
        new PowerVar<Confusion>(6),
        new PowerVar<Flashback>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("FlashbackAmount").WithMultiplier((card, _) =>
            card.CombatState!.HittableEnemies.Count() * card.DynamicVars["Flashback"].BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<Flashback>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var enemyCount = card.CombatState.HittableEnemies.Count();
        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, card.DynamicVars["Confusion"].BaseValue, card);

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
            .Execute(choiceContext);

        await SlalMechanicsHelper.GainFlashback(choiceContext, card.Owner.Creature, card.DynamicVars["Flashback"].BaseValue * enemyCount, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
    }
}
