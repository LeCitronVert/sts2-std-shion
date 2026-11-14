using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Royal Scheme (Team-Up), on the Regent: this turn, whenever your Partner plays a Colorless card,
/// add a copy of it that costs 0 into your Hand (one copy per stack).
/// </summary>
public class RoyalScheme : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public static bool IsColorless(CardModel card) =>
        card.Type is not (CardType.Status or CardType.Curse) && card.Pool.IsColorless;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (Owner.IsDead || Owner.Player is not { } owner || !Partners.Contains(card.Owner.Creature) || !IsColorless(card)) return;

        Flash();
        for (var i = 0; i < Amount; i++)
        {
            var copy = SlalMechanicsHelper.CreateCopyForPlayer(card, owner);
            if (!copy.EnergyCost.CostsX)
            {
                copy.EnergyCost.SetThisCombat(0);
            }
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, owner);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
