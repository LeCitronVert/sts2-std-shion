using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 4 damage to a random enemy for each card you played this turn (this one not included). (Retain)
/// </summary>
public class SlalPinballMove() : SlalTheEndlessCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4M, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((card, _) => CountCardsPlayedThisTurn(card)),
    ];

    private static int CountCardsPlayedThisTurn(CardModel card)
    {
        if (card.CombatState == null) return 0;
        return CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Card.Owner == card.Owner && e.CardPlay.IsFirstInSeries);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // This play hasn't finished yet, so it isn't counted
        var hits = CountCardsPlayedThisTurn(card);
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
        AddKeyword(CardKeyword.Retain);
    }
}
