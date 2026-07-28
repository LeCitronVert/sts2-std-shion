using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    
    public static async Task ExhaustAllTeamBlasts(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        foreach (Creature creature in card.CombatState?.GetTeammatesOf(card.Owner.Creature).Where(c => c?.IsPlayer ?? false)!)
        {
            if (creature.Player != null)
                ShionMechanicsHelper.GetAllCombatCards(creature.Player)
                    .Where(c => c.Keywords.Contains(DreamersKeywords.TeamBlast)).ToList().ForEach(c =>
                    {
                        CardCmd.Exhaust(
                            choiceContext,
                            c
                        );
                    });
        }
    }
}