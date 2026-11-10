using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Add a copy of the first X 0-cost Attacks you play each turn into your Hand.
/// "0-cost" means no energy was paid for that play (X-cost played for 0, free or autoplayed Attacks count).
/// </summary>
public class ComboMeter : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _triggersThisTurn;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;
        // Only once per play, even when the card is replayed.
        if (cardPlay.PlayIndex != 0 || cardPlay.Resources.EnergySpent != 0) return;
        if (_triggersThisTurn >= Amount || Owner.CombatState is null) return;

        _triggersThisTurn++;
        Flash();
        var copy = Owner.CombatState.CreateCard(cardPlay.Card.CanonicalInstance, Owner.Player!);
        if (cardPlay.Card.IsUpgraded) CardCmd.Upgrade(copy);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner.Player));
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side) _triggersThisTurn = 0;
        return Task.CompletedTask;
    }
}
