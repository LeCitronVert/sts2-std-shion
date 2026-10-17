using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Random. Deal damage to a random enemy Random times.
/// Whenever a hit makes an enemy lose no HP, increase this card's damage this combat.
/// </summary>
public class SlalDiceShotgun() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    private decimal _extraDamageFromWhiffs;

    /// <summary>
    /// Required so we can restore the extra damage amount after a downgrade (see Rampage).
    /// </summary>
    private decimal ExtraDamageFromWhiffs
    {
        get => _extraDamageFromWhiffs;
        set
        {
            AssertMutable();
            _extraDamageFromWhiffs = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1M, ValueProp.Move),
        new DynamicVar("Increase", 2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Random>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, 1, card);

        var hits = SlalMechanicsHelper.GetRandom(card.Owner.Creature);
        for (var i = 0; i < hits; i++)
        {
            var enemy = card.Owner.RunState.Rng.CombatTargets.NextItem(card.CombatState.HittableEnemies);
            if (enemy == null) break;

            var attack = await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .FromCard(card)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);

            var whiffs = attack.Results.SelectMany(r => r).Count(r => r.UnblockedDamage <= 0);
            if (whiffs <= 0) continue;

            var increase = whiffs * card.DynamicVars["Increase"].BaseValue;
            card.DynamicVars.Damage.BaseValue += increase;
            card.ExtraDamageFromWhiffs += increase;
        }
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += ExtraDamageFromWhiffs;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
