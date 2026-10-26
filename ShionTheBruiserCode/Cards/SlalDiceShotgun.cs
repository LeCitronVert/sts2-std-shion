using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// For each card in your Hand, gain 1 Random and deal damage equal to its energy cost (0 for X-cost) to a random enemy.
/// </summary>
public class SlalDiceShotgun() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RandomGain").WithMultiplier((card, _) => PreviewHelper.OtherCardsInHand(card).Count),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Random>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // One attack context for all hits, so Vigor/Strength/Sharp etc. apply to every hit (Vigor consumed once).
        await using var context = await AttackCommand.CreateContextAsync(card.CombatState, choiceContext, card);
        var animPlayed = false;

        foreach (var handCard in PileType.Hand.GetPile(card.Owner).Cards.ToList())
        {
            await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, 1, card);

            var damage = SlalMechanicsHelper.GetEnergyCost(handCard);

            var enemy = card.Owner.RunState.Rng.CombatTargets.NextItem(card.CombatState.HittableEnemies);
            if (enemy == null) continue;

            if (!animPlayed)
            {
                await CreatureCmd.TriggerAnim(card.Owner.Creature, "Attack", card.Owner.Character.AttackAnimDelay);
                animPlayed = true;
            }

            VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_blunt");
            context.AddHit(await CreatureCmd.Damage(choiceContext, enemy, damage, ValueProp.Move, card));
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
