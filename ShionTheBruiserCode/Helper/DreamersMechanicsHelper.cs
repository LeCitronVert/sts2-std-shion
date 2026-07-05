using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class DreamersMechanicsHelper
{
    public static Task ExhaustAllEmeraldPowers(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ShionMechanicsHelper.GetAllCombatCards(cardPlay.Card.Owner).Where(c => c.Keywords.Contains(DreamersKeywords.EmeraldPower)).ToList().ForEach(c =>
        {
            CardCmd.Exhaust(
                choiceContext,
                c
            );
        });
        
        return Task.CompletedTask;
    }
}