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
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

[Pool(typeof(TokenCardPool))]
public class SlalFreshIdea(): SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new InCombatPowerAmountVar<CreativeSpirit>("InCombatHasCreativeSpirit"),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.Static(StaticHoverTip.Transform),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        List<CardModel> chosenCards;
        if (!IsUpgraded)
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
        
        // Slal's generable tokens (Bumper, Ventilator, Squeaky Squirrel Form...) can also be created.
        // Tokens live in the TokenCardPool, so they are added separately from the regular options.
        var cardOptions = SlalMechanicsHelper.GetRandomCardOptions(Owner)
            .Concat(ModelDb.CardPool<TokenCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedInCombat && c is not SlalFreshIdea && (
                SlalMechanicsHelper.HasRandomCardRarity(Owner, c)
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