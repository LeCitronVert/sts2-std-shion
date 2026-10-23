using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Transform up to 2 (3) cards in your Hand into Slimed. Gain 1 Max HP for each card transformed this way. Exhaust.
/// </summary>
public class SlalAbsorption() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
        new MaxHpVar(1),
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

        var originals = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, card.DynamicVars.Cards.IntValue),
            c => c.IsTransformable,
            card
        )).ToList();
        if (originals.Count == 0) return;

        foreach (var original in originals)
        {
            await CardCmd.Transform(original, card.CombatState.CreateCard<Slimed>(card.Owner));
        }
        await CreatureCmd.GainMaxHp(card.Owner.Creature, card.DynamicVars.MaxHp.IntValue * originals.Count);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
