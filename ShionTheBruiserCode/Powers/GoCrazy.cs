using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// X times per turn, you can play a card in your Hand you don't have the energy for:
/// you spend all your energy and lose HP equal to the energy you're missing.
/// Handled in <see cref="Patches.GoCrazyPatches"/>; the counter shows the uses left this turn.
/// </summary>
public class GoCrazy : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => Math.Max(0, Amount - _usedThisTurn);

    private int _usedThisTurn;
    private CardModel? _pendingCard;
    private int _pendingHpLoss;

    /// <summary>Whether this power can pay the missing energy of this card right now.</summary>
    public bool CanPayFor(CardModel card) =>
        card.Owner.Creature == Owner && !card.EnergyCost.CostsX && DisplayAmount > 0;

    /// <summary>Called right before the card spends its energy. The HP is lost once the energy is spent.</summary>
    public void PayFor(CardModel card, int missingEnergy)
    {
        _usedThisTurn++;
        _pendingCard = card;
        _pendingHpLoss = missingEnergy;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (card != _pendingCard) return;

        var hpLoss = _pendingHpLoss;
        _pendingCard = null;
        _pendingHpLoss = 0;
        Flash();
        await SeelsMechanicsHelper.LoseHp(new ThrowingPlayerChoiceContext(), Owner, hpLoss, null);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            _usedThisTurn = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
