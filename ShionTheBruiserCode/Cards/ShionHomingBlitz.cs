using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionHomingBlitz() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var amountOfEnemies = play.Card.CombatState?.HittableEnemies.Count ?? 0;

        if (amountOfEnemies <= 0) return;
        if (card.CombatState != null)
            await DamageCmd
                .Attack(card.DynamicVars.Damage.BaseValue)
                .FromCard(card)
                .TargetingAllOpponents(card.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
                .WithHitCount(amountOfEnemies)
                .Execute(choiceContext)
            ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}