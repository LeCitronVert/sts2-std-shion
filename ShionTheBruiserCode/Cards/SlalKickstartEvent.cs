using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 10 damage to ALL enemies. Fill your Hand with Fresh Idea+. This turn, whenever you play a Novel card, gain 1 energy.
/// Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalKickstartEvent() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10M, ValueProp.Move),
        new PowerVar<KickstartEvent>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(true),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
    ];

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

        var amount = Math.Max(0, CardPile.MaxCardsInHand - PileType.Hand.GetPile(card.Owner).Cards.Count);
        List<CardModel> ideas = [];
        for (var i = 0; i < amount; i++)
        {
            ideas.Add(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, true));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(ideas, PileType.Hand, card.Owner);

        await PowerCmd.Apply<KickstartEvent>(choiceContext, card.Owner.Creature, card.DynamicVars["KickstartEvent"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
