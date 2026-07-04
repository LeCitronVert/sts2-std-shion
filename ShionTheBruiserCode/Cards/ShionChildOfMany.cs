using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionChildOfMany() : ShionTheBruiserCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var pools = ModelDb.AllCharacterCardPools.Where(p => p.Id != ModelDb.CardPool<ShionTheBruiserCardPool>().Id).ToList();
        
        List<CardModel> powerCards = [];
        foreach (var pool in pools)
        {
            powerCards = powerCards
                .Concat(pool.GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint))
                .ToList()
            ;
        }
        
        powerCards = powerCards.Where(c => c.Type == CardType.Power && c.IsTransformable).ToList();
        
        CardModel? chosenPower = await CardSelectCmd.FromChooseACardScreen(
            choiceContext, 
            CardFactory.GetDistinctForCombat(
                card.Owner, 
                powerCards, 
                3, 
                card.Owner.RunState.Rng.CombatCardGeneration).ToList(), 
            card.Owner
            );

        if (chosenPower == null) return;
        await CardCmd.AutoPlay(choiceContext, chosenPower, null);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}