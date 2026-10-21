using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of your turn in X turns, the effect of the card that created this triggers.
/// Does not stack: each Request creates its own Requested counter.
/// </summary>
public class Requested : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public CardModel? SourceCard { get; private set; }

    private Func<PlayerChoiceContext, Task>? _effect;
    private bool _triggered;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        SourceCard is null ? [] : [HoverTipFactory.FromCard(SourceCard)];

    public void Setup(CardModel sourceCard, Func<PlayerChoiceContext, Task> effect)
    {
        SourceCard = sourceCard;
        _effect = effect;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        await Advance(choiceContext, 1);
    }

    /// <summary>
    /// Decreases the counter by X. Triggers the effect immediately when it reaches 0.
    /// </summary>
    public async Task Advance(PlayerChoiceContext choiceContext, int amount)
    {
        if (_triggered || amount <= 0) return;

        if (Amount > amount)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -amount, Owner, null, true);
            return;
        }

        _triggered = true;
        Flash();
        await PowerCmd.Remove(this);

        if (Owner.CombatState is not null)
        {
            foreach (var listener in Owner.CombatState.IterateHookListeners().OfType<IFryListener>().ToList())
            {
                await listener.AfterRequestCounterReachedZero(choiceContext, this);
            }
        }

        if (_effect is not null && Owner.IsAlive)
        {
            await _effect(choiceContext);
        }
    }

    /// <summary>
    /// Removes this counter without triggering its effect.
    /// </summary>
    public async Task Cancel()
    {
        _triggered = true;
        await PowerCmd.Remove(this);
    }
}
