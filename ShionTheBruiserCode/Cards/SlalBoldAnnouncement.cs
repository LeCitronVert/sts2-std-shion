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
/// Deal 1 damage and apply 6 Confusion to ALL enemies. Gain 1 Memory this turn for each enemy.
/// </summary>
public class SlalBoldAnnouncement() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1M, ValueProp.Move),
        new PowerVar<Confusion>(6),
        new PowerVar<MemoryThisTurn>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("MemoryAmount").WithMultiplier((card, _) =>
            card.CombatState!.HittableEnemies.Count() * card.DynamicVars["MemoryThisTurn"].BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<Memory>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var enemyCount = card.CombatState.HittableEnemies.Count();
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
            .Execute(choiceContext);

        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, card.DynamicVars["Confusion"].BaseValue, card);
        await SlalMechanicsHelper.GainMemoryThisTurn(choiceContext, card.Owner.Creature, card.DynamicVars["MemoryThisTurn"].BaseValue * enemyCount, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
    }
}
