using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Fill your Hand with Steady Fresh Ideas.
/// Deal damage and apply Confusion to ALL enemies equal to the number of cards created this way.
/// </summary>
public class SlalKickstartEvent() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => FreeSlots(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromCard<SlalFreshIdea>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Steady>(),
    ];

    private static int FreeSlots(CardModel card) =>
        Math.Max(0, CardPile.MaxCardsInHand - PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card));

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var amount = FreeSlots(card);
        List<CardModel> ideas = [];
        for (var i = 0; i < amount; i++)
        {
            var idea = SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded);
            SlalMechanicsHelper.MakeSteady(idea);
            ideas.Add(idea);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(ideas, PileType.Hand, card.Owner);

        if (ideas.Count == 0) return;

        await DamageCmd.Attack(ideas.Count)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, ideas.Count, card);
    }

    protected override void OnUpgrade()
    {
    }
}
