using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 7 damage to ALL enemies. For each enemy, gain 1 Thorns if it didn't lose HP,
/// otherwise add a random Power into your Discard Pile.
/// </summary>
public class SlalBoldAnnouncement() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7M, ValueProp.Move),
        new PowerVar<ThornsPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ThornsPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var enemies = card.CombatState.HittableEnemies.ToList();
        var attack = await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
            .Execute(choiceContext);

        var results = attack.Results.SelectMany(r => r).ToList();
        var enemiesThatLostHp = enemies.Count(e => results.Any(r => r.Receiver == e && r.UnblockedDamage > 0));
        var enemiesThatDidNot = enemies.Count - enemiesThatLostHp;

        if (enemiesThatDidNot > 0)
        {
            await PowerCmd.Apply<ThornsPower>(choiceContext, card.Owner.Creature, card.DynamicVars["ThornsPower"].BaseValue * enemiesThatDidNot, card.Owner.Creature, card);
        }

        if (enemiesThatLostHp > 0)
        {
            var slalPowers = ModelDb.CardPool<SlalTheEndlessCardPool>()
                .GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.Type == CardType.Power);

            List<CardModel> powers = [];
            for (var i = 0; i < enemiesThatLostHp; i++)
            {
                powers.AddRange(CardFactory.GetForCombat(card.Owner, slalPowers, 1, card.Owner.RunState.Rng.CombatCardGeneration));
            }
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(powers, PileType.Discard, card.Owner));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars["ThornsPower"].UpgradeValueBy(1);
    }
}
