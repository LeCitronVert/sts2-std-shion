using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Team-Up cards: multiplayer cards played by two players, the Initiator (whose pool has the card) and a Partner.
/// </summary>
public static class TeamUpHelper
{
    public static IEnumerable<ShionTeamUpCard> AllTeamUps => ModelDb.AllCards.OfType<ShionTeamUpCard>();

    public static bool IsPartnerCharacter(ShionTeamUpCard card, CharacterModel character)
    {
        return card.PartnerCharacter == null || card.PartnerCharacter.Id == character.Id;
    }

    /// <summary>
    /// Loot only offers a Team-Up when the run has its Initiator's class, and another player of its Partner's class.
    /// </summary>
    public static bool IsAvailableInRun(ShionTeamUpCard card, IRunState runState)
    {
        return runState.Players.Any(initiator => initiator.Character.Id == card.InitiatorCharacter.Id
            && runState.Players.Any(partner => partner != initiator && IsPartnerCharacter(card, partner.Character)));
    }

    /// <summary>
    /// Team-Ups this player can initiate with the current team.
    /// </summary>
    public static IEnumerable<ShionTeamUpCard> GetAvailableTeamUps(Player player)
    {
        return AllTeamUps.Where(card => card.InitiatorCharacter.Id == player.Character.Id
            && player.RunState.Players.Any(partner => partner != player && IsPartnerCharacter(card, partner.Character)));
    }

    /// <summary>
    /// Living players who can play the Partner's copy of this card.
    /// </summary>
    public static IEnumerable<Player> GetPartners(ShionTeamUpCard card, Player initiator)
    {
        return initiator.RunState.Players.Where(p => p != initiator
            && p.Creature.IsAlive
            && p.Creature.CombatState != null
            && p.Creature.CombatState == initiator.Creature.CombatState
            && IsPartnerCharacter(card, p.Character));
    }

    /// <summary>
    /// Class named on the "Team-Up: (Class)" line, from "SHIONTHEBRUISER-TEAMUP.class.(Character ID)" ("Ironclad" rather than "The Ironclad").
    /// "Anyone" when any class can be the Partner.
    /// </summary>
    public static string GetClassName(CharacterModel? character)
    {
        if (character == null)
        {
            return new LocString("card_keywords", "SHIONTHEBRUISER-TEAMUP.anyone").GetFormattedText();
        }

        var key = "SHIONTHEBRUISER-TEAMUP.class." + character.Id.Entry;
        return LocString.Exists("card_keywords", key)
            ? new LocString("card_keywords", key).GetFormattedText()
            : character.Title.GetFormattedText();
    }
}
