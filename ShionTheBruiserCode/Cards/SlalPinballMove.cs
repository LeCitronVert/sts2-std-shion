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
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Deal 4 damage to a random enemy for each different card you played this turn (this one not included).
/// Upgraded: each card counts, even identical ones.
/// </summary>
public class SlalPinballMove() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4M, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((card, _) => CountCardsPlayedThisTurn(card)),
    ];

    private static int CountCardsPlayedThisTurn(CardModel card)
    {
        if (card.CombatState == null) return 0;
        var played = CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Card.Owner == card.Owner && e.CardPlay.IsFirstInSeries)
            .Select(e => e.CardPlay.Card)
            .ToList();
        return card.IsUpgraded ? played.Count : SlalMechanicsHelper.CountDifferentCards(played);
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
    }
}
