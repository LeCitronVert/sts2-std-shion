using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Regent: fill your partner's Hand with random Colorless cards.
/// This turn, whenever they play a Colorless card, add a 0-cost copy of it into your Hand. Exhaust. (Upgraded: Retain.)
/// </summary>
public class RegentRoyalScheme() : RegentTeamUpCard(0,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RoyalScheme>()];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        // This copy is still in the Hand while it resolves, so only count the other cards
        var room = CardPile.MaxCardsInHand - PileType.Hand.GetPile(partner).Cards.Count(c => c != this);
        if (room > 0)
        {
            var options = ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(partner.UnlockState, partner.RunState.CardMultiplayerConstraint);
            var cards = CardFactory.GetDistinctForCombat(partner, options, room, partner.RunState.Rng.CombatCardGeneration).ToList();
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, partner);
        }

        if (initiator.Creature.IsAlive)
        {
            await TeamUpPower.Apply<RoyalScheme>(choiceContext, initiator.Creature, partner.Creature, 1, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
