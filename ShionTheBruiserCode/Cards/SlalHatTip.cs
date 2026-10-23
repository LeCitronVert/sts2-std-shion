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
/// Deal damage equal to the enemy's Confusion.
/// If you have as many cards in your Hand as your Random, upgrade those cards.
/// </summary>
public class SlalHatTip() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((_, target) => target?.GetPowerAmount<Confusion>() ?? 0),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && HandMatchesRandom(excludeThis: true);

    private bool HandMatchesRandom(bool excludeThis)
    {
        var handCount = PileType.Hand.GetPile(Owner).Cards.Count(c => !excludeThis || c != this);
        return handCount > 0 && handCount == SlalMechanicsHelper.GetRandom(Owner.Creature);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        // This card has left the Hand by now
        if (!HandMatchesRandom(excludeThis: false)) return;
        foreach (var handCard in PileType.Hand.GetPile(card.Owner).Cards.Where(c => c.IsUpgradable).ToList())
        {
            CardCmd.Upgrade(handCard);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
