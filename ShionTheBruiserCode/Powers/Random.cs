using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Random : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public const int GetCardAt = 10;

    public override int DisplayAmount => GetCurrentAmount();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DisplayAmount", 0)
    ];
    
    public int GetCurrentAmount()
    {
        var mod = (int)Amount % GetCardAt;
        return mod == 0 ? GetCardAt : mod;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        
        var previousAmount = Amount - amount;
        await TriggerCardCreation(previousAmount, Amount);
    }

    private async Task TriggerCardCreation(decimal previousAmount, decimal newAmount)
    {
        if (Owner.Player is null) return;

        var thresholdsCrossed = (int)(newAmount / GetCardAt) - (int)(previousAmount / GetCardAt);
        if (thresholdsCrossed <= 0) return;

        for (var i = 0; i < thresholdsCrossed; i++)
        {
            var card = CardFactory.GetDistinctForCombat(
                Owner.Player,
                GetCharacterCardsForCreation(),
                1,
                Owner.Player.RunState.Rng.CombatCardGeneration
            ).First();
            card.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
        }
    }

    private IEnumerable<CardModel> GetCharacterCardsForCreation()
    {
        if (Owner.Player is null) return [];

        return Owner.Player.Character.CardPool.GetUnlockedCards(Owner.Player.UnlockState,
            Owner.Player.RunState.CardMultiplayerConstraint);
    }
}