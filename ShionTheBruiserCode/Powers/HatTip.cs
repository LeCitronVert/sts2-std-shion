using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a card that creates Goofball Token Powers, add X random Goofball Token Powers into your Hand.
/// They are free to play this turn.
/// </summary>
public class HatTip : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private class Data
    {
        /// <summary>Cards still resolving, and whether they created a Goofball Token Power.</summary>
        public readonly Dictionary<CardModel, bool> playsInProgress = [];

        /// <summary>True while Hat Tip adds its own powers, so they don't trigger it again.</summary>
        public bool adding;
    }

    protected override object InitInternalData() => new Data();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && cardPlay.IsFirstInSeries)
        {
            GetInternalData<Data>().playsInProgress[cardPlay.Card] = false;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        var data = GetInternalData<Data>();
        if (creator != Owner.Player || data.adding || !SlalMechanicsHelper.IsGoofballTokenPower(card)) return Task.CompletedTask;

        foreach (var playedCard in data.playsInProgress.Keys.ToList())
        {
            data.playsInProgress[playedCard] = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.IsLastInSeries) return;
        var data = GetInternalData<Data>();
        if (!data.playsInProgress.Remove(cardPlay.Card, out var createdGoofball) || !createdGoofball) return;
        if (Owner.Player is null) return;

        Flash();
        data.adding = true;
        try
        {
            for (var i = 0; i < Amount; i++)
            {
                var power = SlalMechanicsHelper.CreateGoofballTokenPower(Owner.Player, false);
                if (!power.EnergyCost.CostsX)
                {
                    power.SetToFreeThisTurn();
                }
                await CardPileCmd.AddGeneratedCardToCombat(power, PileType.Hand, Owner.Player);
            }
        }
        finally
        {
            data.adding = false;
        }
    }
}
