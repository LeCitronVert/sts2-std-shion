using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Transform a card in your Hand into Slimed. Gain 1 Max HP, plus Max HP equal to the energy cost of that card.
/// Very Slimy. (Retain)
/// </summary>
public class SlalAbsorption() : SlalSlimyCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.VerySlimy, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
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

        var original = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
            c => c.IsTransformable,
            card
        )).FirstOrDefault();

        var extraMaxHp = 0;
        if (original != null)
        {
            // Read the cost before the card is gone
            extraMaxHp = SlalMechanicsHelper.GetEnergyCost(original);
            await CardCmd.Transform(original, card.CombatState.CreateCard<Slimed>(card.Owner));
        }

        await CreatureCmd.GainMaxHp(card.Owner.Creature, card.DynamicVars.MaxHp.IntValue + extraMaxHp);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
