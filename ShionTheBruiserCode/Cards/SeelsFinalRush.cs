using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 2 damage to a random enemy for each Slash in your Exhaust Pile. Add Finishing Explosion to your Hand. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SeelsFinalRush() : SeelsTheRecklessCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.RandomEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(2M, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("Hits").WithMultiplier((card, _) => SeelsFinishingExplosion.CountExhaustedSlashes(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(),
        HoverTipFactory.FromCard<SeelsFinishingExplosion>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        var hits = SeelsFinishingExplosion.CountExhaustedSlashes(this);
        if (hits > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingRandomOpponents(CombatState)
                .WithHitCount(hits)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }

        var explosion = CombatState.CreateCard<SeelsFinishingExplosion>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(explosion, PileType.Hand, Owner));
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
