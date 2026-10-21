using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain Random equal to the number of cards in your Hand.
/// Deal damage equal to your Random to ALL enemies.
/// </summary>
public class SlalDiceShotgun() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RandomGain").WithMultiplier((card, _) => PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card) + 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Random>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // + 1 for this card, which has already left the Hand
        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, PileType.Hand.GetPile(card.Owner).Cards.Count + 1, card);

        var damage = SlalMechanicsHelper.GetRandom(card.Owner.Creature);
        if (damage <= 0) return;

        await DamageCmd.Attack(damage)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
