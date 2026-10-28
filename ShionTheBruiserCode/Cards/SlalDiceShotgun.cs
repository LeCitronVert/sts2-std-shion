using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal damage equal to your Random to ALL enemies. Gain Random equal to the number of different cards in your Hand.
/// </summary>
public class SlalDiceShotgun() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => SlalMechanicsHelper.GetRandom(card.Owner.Creature)),
        new CalculationExtraVar(1),
        new CalculatedVar("RandomGain").WithMultiplier((card, _) => SlalMechanicsHelper.CountDifferentCards(PreviewHelper.OtherCardsInHand(card))),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Random>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        var different = SlalMechanicsHelper.CountDifferentCards(PileType.Hand.GetPile(card.Owner).Cards);
        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, different, card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
