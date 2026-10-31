using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Helpers;
using Godot;
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

    private const string DaggerSpraySfx = "event:/sfx/characters/silent/silent_dagger_spray";
    private static readonly Color DaggerSprayColor = new("#b1ccca");

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // Dagger Spray's sound and visuals
        SfxCmd.Play(DaggerSpraySfx);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(card.Owner.Creature, DaggerSprayColor, goingRight: true))
            .BeforeDamage(() =>
            {
                foreach (var enemy in card.CombatState.HittableEnemies)
                {
                    var impact = NDaggerSprayImpactVfx.Create(enemy, DaggerSprayColor, goingRight: true);
                    if (impact != null) NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(impact);
                }
                return Task.CompletedTask;
            })
            .Execute(choiceContext);

        var different = SlalMechanicsHelper.CountDifferentCards(PileType.Hand.GetPile(card.Owner).Cards);
        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, different, card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
