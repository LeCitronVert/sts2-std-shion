using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalFreshIdea(): SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<Random>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            null
        );

        var canChooseCard = IsUpgraded;
        CardModel? chosenCard = null;

        if (!canChooseCard)
        {
            chosenCard = SlalMechanicsHelper.ChooseRandomCardWithoutNovelIfPossible(Owner, PileType.Hand);
        }
        else
        {
            chosenCard = (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
                null,
                card
            )).FirstOrDefault();
        }

        if (chosenCard == null) return;
        
        var cardOptions = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.CanBeGeneratedInCombat && c.Rarity is CardRarity.Common or CardRarity.Uncommon)
            .ToList()
        ;
        
        await CardCmd.Transform(
            new CardTransformation(chosenCard, cardOptions).Yield(),
            Owner.RunState.Rng.CombatCardSelection
        );
    }

    protected override void OnUpgrade()
    {
    }
}