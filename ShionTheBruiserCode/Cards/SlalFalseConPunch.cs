using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal X damage and apply Y Confusion to a random enemy for each card in your Hand.
/// </summary>
public class SlalFalseConPunch() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2M, ValueProp.Move),
        new PowerVar<Confusion>(4),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("Hits").WithMultiplier((card, _) => PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card) + 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // + 1 for this card, which has already left the Hand
        var hits = PileType.Hand.GetPile(card.Owner).Cards.Count + 1;
        for (var i = 0; i < hits; i++)
        {
            var enemy = card.Owner.RunState.Rng.CombatTargets.NextItem(card.CombatState.HittableEnemies);
            if (enemy == null) break;

            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .FromCard(card)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);

            if (enemy.IsAlive)
            {
                await PowerCmd.Apply<Confusion>(choiceContext, enemy, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars["Confusion"].UpgradeValueBy(2);
    }
}
