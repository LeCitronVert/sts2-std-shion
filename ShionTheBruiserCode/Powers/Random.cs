using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Random : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public const int GetCardAt = 10;
    public const int MemoryPerThreshold = 1;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>()
    ];

    public override int DisplayAmount => GetCurrentAmount();

    /// <summary>
    /// Number of times Random was gained this combat (used by Conclusion).
    /// </summary>
    private int _timesGained;
    public int TimesGained
    {
        get => _timesGained;
        private set
        {
            AssertMutable();
            _timesGained = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DisplayAmount", 0)
    ];

    public int GetCurrentAmount()
    {
        var mod = (int)Amount % GetCardAt;
        return mod == 0 ? GetCardAt : mod;
    }

    /// <summary>
    /// Sets the Random back to the given amount (Tabula Rasa).
    /// </summary>
    public async Task DropTo(PlayerChoiceContext choiceContext, int amount)
    {
        var offset = amount - Amount;
        if (offset == 0) return;
        await PowerCmd.ModifyAmount(choiceContext, this, offset, Owner, null);
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        // This hook is called for every power in combat, only react to this one.
        if (power != this) return;

        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        InvokeDisplayAmountChanged();

        if (amount <= 0) return;

        TimesGained++;
        await SlalMechanicsHelper.TriggerRandomGained(choiceContext, Owner, amount);

        var previousAmount = Amount - amount;
        await TriggerThresholds(choiceContext, previousAmount, Amount);
    }

    /// <summary>
    /// Each time Random reaches 10, gain 1 Memory (Random then drops back to 1 on the next gain).
    /// </summary>
    private async Task TriggerThresholds(PlayerChoiceContext choiceContext, decimal previousAmount, decimal newAmount)
    {
        var thresholdsCrossed = (int)(newAmount / GetCardAt) - (int)(previousAmount / GetCardAt);
        if (thresholdsCrossed <= 0) return;

        for (var i = 0; i < thresholdsCrossed; i++)
        {
            await SlalMechanicsHelper.GainMemory(choiceContext, Owner, MemoryPerThreshold);
            await SlalMechanicsHelper.TriggerRandomThresholdReached(choiceContext, Owner);
        }
    }
}
