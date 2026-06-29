using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class ShieldBreaker : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // Stored between BeforeCardPlayed and AfterCardPlayed.
    private CardModel? _pendingCard;
    private Creature? _pendingTarget;

    // Prevents replayed clones from re-triggering this power.
    private bool _isReplaying;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // Only react to the owner's own attacks, not re-entrant replays or
        // attacks that were already queued, and only when the target has block.
        if (
            _isReplaying
            || _pendingCard != null
            || cardPlay.Card.Owner.Creature != Owner
            || cardPlay.Card.Type != CardType.Attack
            || (cardPlay.Target?.Block ?? 0) <= 0
        ) return Task.CompletedTask;

        _pendingCard = cardPlay.Card;
        _pendingTarget = cardPlay.Target;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != _pendingCard) return;

        var target = _pendingTarget;
        _pendingCard = null;
        _pendingTarget = null;

        if (target == null || !target.IsAlive) return;

        Flash();
        _isReplaying = true;
        try
        {
            for (var i = 0; i < Amount; i++)
            {
                if (!target.IsAlive) break;
                var clone = cardPlay.Card.CreateClone();
                await CardCmd.AutoPlay(choiceContext, clone, target);
            }
        }
        finally
        {
            _isReplaying = false;
        }
    }
}


