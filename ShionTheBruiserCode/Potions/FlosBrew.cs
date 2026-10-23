using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Add 3 random Rare Form cards of any color into your Hand. Choose one to be free to play this turn.
/// Pony Form, Television Form and Sparrow Form (from Creativity Unbound) can also be picked.
/// </summary>
public class FlosBrew : SlalTheEndlessPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    private static IReadOnlyList<CardModel> CreativityUnboundForms => [
        ModelDb.Card<SlalPonyForm>(),
        ModelDb.Card<SlalTelevisionForm>(),
        ModelDb.Card<SlalSparrowForm>(),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        List<CardPoolModel> pools = [
            ..ModelDb.AllCharacterCardPools,
            ModelDb.CardPool<ShionTheBruiserCardPool>(),
            ModelDb.CardPool<SlalTheEndlessCardPool>(),
        ];

        var forms = pools
            .DistinctBy(p => p.Id)
            .SelectMany(p => p.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Rarity == CardRarity.Rare && SlalMechanicsHelper.IsFormPower(c))
            .Concat(CreativityUnboundForms)
            .DistinctBy(c => c.Id);

        var cards = CardFactory.GetDistinctForCombat(Owner, forms, 3, Owner.RunState.Rng.CombatCardGeneration).ToList();
        if (cards.Count == 0) return;

        var chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, Owner);
        chosen?.SetToFreeThisTurn();

        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner);
    }
}
