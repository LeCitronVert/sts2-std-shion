using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalFreshIdea(): SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new InCombatHasCreativeSpiritVar(this),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.Static(StaticHoverTip.Transform),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        // Television Form: grant additional Random and transform ALL cards in Hand.
        var televisionAmount = Owner.Creature.GetPowerAmount<TelevisionForm>();
        await PowerCmd.Apply<Random>(
            choiceContext,
            Owner.Creature,
            1 + televisionAmount,
            Owner.Creature,
            null
        );

        List<CardModel> chosenCards;
        if (televisionAmount > 0)
        {
            chosenCards = PileType.Hand.GetPile(Owner).Cards.Where(c => c.IsTransformable).ToList();
        }
        else if (!IsUpgraded)
        {
            var randomCard = SlalMechanicsHelper.ChooseRandomCardWithoutNovelIfPossible(Owner, PileType.Hand);
            chosenCards = randomCard == null ? [] : [randomCard];
        }
        else
        {
            chosenCards = (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
                null,
                card
            )).ToList();
        }

        if (chosenCards.Count == 0) return;
        
        // Check if CreativeSpirit power is active to allow Rare cards
        var hasCreativeSpiritPower = Owner.Creature.Powers.Any(p => p is CreativeSpirit);
        
        // Slal's generable tokens (Bumper, Ventilator, Squeaky Squirrel Form...) can also be created.
        var cardOptions = SlalMechanicsHelper.GetRandomCardOptions(Owner)
            .Where(c => c.CanBeGeneratedInCombat && c is not SlalFreshIdea && (
                (hasCreativeSpiritPower 
                    ? c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare
                    : c.Rarity is CardRarity.Common or CardRarity.Uncommon)
                || (c.Rarity == CardRarity.Token && SlalMechanicsHelper.IsNativeToSlal(c))))
            .ToList()
        ;
        
        await CardCmd.Transform(
            chosenCards.Select(c => new CardTransformation(c, cardOptions)),
            Owner.RunState.Rng.CombatCardSelection
        );
    }

    protected override void OnUpgrade()
    {
    }
}