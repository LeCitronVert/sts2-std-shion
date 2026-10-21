using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Draw 1 (2) card(s). If you draw an Attack, a random enemy takes 3 damage (for each Attack drawn this way).
/// </summary>
public class SeelsPrivateJoke() : SeelsTheRecklessCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3M, ValueProp.Move),
        new CardsVar(1),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        var drawn = (await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner)).ToList();
        var attacks = drawn.Count(c => c.Type == CardType.Attack);
        var hits = IsUpgraded ? attacks : Math.Min(attacks, 1);
        if (hits <= 0) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1M);
    }
}
