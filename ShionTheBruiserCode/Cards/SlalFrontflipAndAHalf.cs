using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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
/// Deal 5 damage and apply 1 Weak to ALL enemies and yourself.
/// If you lost HP this turn, ALL enemies gain 5 Confusion.
/// </summary>
public class SlalFrontflipAndAHalf() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5M, ValueProp.Move),
        new PowerVar<WeakPower>(1),
        new PowerVar<Confusion>(5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && LostHpThisTurn(this);

    private static bool LostHpThisTurn(CardModel card)
    {
        return CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Any(e => e.Receiver == card.Owner.Creature && e.Result.UnblockedDamage > 0 && e.HappenedThisTurn(card.CombatState));
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await SlalMechanicsHelper.ApplyToAllEnemies<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card);
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        if (LostHpThisTurn(card))
        {
            await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, card.DynamicVars["Confusion"].BaseValue, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars["Confusion"].UpgradeValueBy(2);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}
