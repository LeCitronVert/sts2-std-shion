using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Transform ALL cards (except Slimed) in your Hand into Slimed. Heal 2 HP for each card transformed this way. Exhaust.
/// </summary>
public class SlalQuickMeal(): SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(2),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HealAmount").WithMultiplier((card, _) => ((SlalQuickMeal)card).PreviewHeal())
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<Slimed>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var toTransform = PileType.Hand.GetPile(card.Owner).Cards
            .Where(IsTransformTarget)
            .ToList();
        if (toTransform.Count <= 0) return;

        foreach (var original in toTransform)
        {
            await CardCmd.Transform(original, card.CombatState.CreateCard<Slimed>(card.Owner));
        }

        await CreatureCmd.Heal(
            card.Owner.Creature,
            toTransform.Count * card.DynamicVars.Heal.BaseValue
        );
    }

    private static bool IsTransformTarget(CardModel c) => c is not Slimed && c.IsTransformable;

    /// <summary>
    /// HP this card would heal if played now (it has left the Hand by then, so it doesn't count itself).
    /// </summary>
    private decimal PreviewHeal()
    {
        if (CombatState is null) return 0;

        return PileType.Hand.GetPile(Owner).Cards
            .Count(c => c != this && IsTransformTarget(c)) * DynamicVars.Heal.BaseValue;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
