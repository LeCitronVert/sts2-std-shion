using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 4 damage to a random enemy for each different card in your Hand.
/// </summary>
public class SlalFalseConPunch() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4M, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("Hits").WithMultiplier((card, _) => SlalMechanicsHelper.CountDifferentCards(PreviewHelper.OtherCardsInHand(card))),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // This card has already left the Hand
        var hits = SlalMechanicsHelper.CountDifferentCards(PileType.Hand.GetPile(card.Owner).Cards);
        for (var i = 0; i < hits; i++)
        {
            var enemy = card.Owner.RunState.Rng.CombatTargets.NextItem(card.CombatState.HittableEnemies);
            if (enemy == null) break;

            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .FromCard(card)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
