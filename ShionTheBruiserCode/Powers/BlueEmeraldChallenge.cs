using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Blue challenge (on each player): at the end of your turn, Exhaust 1 random card in your Hand per stack.
/// </summary>
public class BlueEmeraldChallenge : EmeraldChallengePower
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Blue;

    protected virtual PileType PileToExhaustFrom => PileType.Hand;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner) || Owner.Player is not { } player) return;

        for (var i = 0; i < Amount; i++)
        {
            var cards = PileToExhaustFrom.GetPile(player).Cards.ToList();
            if (cards.Count == 0) return;

            if (i == 0) Flash();
            var card = player.RunState.Rng.CombatCardSelection.NextItem(cards);
            if (card is null) return;
            await CardCmd.Exhaust(choiceContext, card);
        }
    }
}
