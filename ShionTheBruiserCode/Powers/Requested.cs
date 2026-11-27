using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("Card"),
        new StringVar("Effect"),
        new DynamicVar("Repeats", 1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        SourceCard is null ? [] : [HoverTipFactory.FromCard(SourceCard)];

    /// <param name="repeats">How many times the effect triggers, shown in the tooltip.</param>
    public void Setup(CardModel sourceCard, Func<PlayerChoiceContext, Task> effect, int repeats = 1)
    {
        SourceCard = sourceCard;
        _effect = effect;
        // Display only: the text is captured when the Request is made, so it shows the values the effect was played with
        ((StringVar)DynamicVars["Card"]).StringValue = sourceCard.Title;
        ((StringVar)DynamicVars["Effect"]).StringValue = GetRequestedEffectText(sourceCard);
        DynamicVars["Repeats"].BaseValue = repeats;
    }

    /// <summary>
    /// The lines of the card's text that come after its "Request X" line (the delayed part).
    /// Skips the in-combat "(Request N)" preview line. Falls back to the whole text.
    /// </summary>
    private static string GetRequestedEffectText(CardModel card)
    {
        var description = card.GetDescriptionForPile(PileType.Play);
        if (string.IsNullOrEmpty(description)) return "";

        var marker = new LocString("powers", "SHIONTHEBRUISER-REQUESTED.cardMarker").GetFormattedText();
        var lines = description.Split('\n');
        var requestLine = Array.FindIndex(lines, l => l.Contains(marker));
        if (requestLine < 0) return description;

        var effectLines = lines.Skip(requestLine + 1).SkipWhile(l => l.TrimStart().StartsWith('(')).ToList();
        return effectLines.Count == 0 ? description : string.Join("\n", effectLines);
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
            await NotifyTicked(choiceContext);
            return;
        }

        _triggered = true;
        Flash();
        await PowerCmd.Remove(this);
        await NotifyTicked(choiceContext);

        if (Owner.CombatState is not null)
        {
            foreach (var listener in Owner.CombatState.IterateHookListeners().OfType<IFryListener>().ToList())
            {
                await listener.AfterRequestCounterReachedZero(choiceContext, this);
            }
        }

        if (_effect is not null && Owner.IsAlive && !await PowerfulAndHelpless.TryReplaceRequest(choiceContext, Owner))
        {
            await _effect(choiceContext);
        }
    }

    private async Task NotifyTicked(PlayerChoiceContext choiceContext)
    {
        if (Owner.CombatState is null) return;
        foreach (var listener in Owner.CombatState.IterateHookListeners().OfType<IFryListener>().ToList())
        {
            await listener.AfterRequestCounterTicked(choiceContext, this);
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
