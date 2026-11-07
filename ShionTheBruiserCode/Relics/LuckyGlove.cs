using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Ancients;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Once: you may Reroll a card reward (the base game's Reroll button, as with Driftwood).
/// The rerolled cards are Upgraded. The button stays on your card rewards until you use it.
/// </summary>
public class LuckyGlove : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    private bool _wasUsed;

    public override bool IsUsedUp => _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (_wasUsed) Status = RelicStatus.Disabled;
        }
    }

    // Rewards whose Reroll button comes from a Lucky Glove (each reward belongs to one player, so to one glove)
    private static readonly ConditionalWeakTable<CardReward, object> _granted = new();

    public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || WasUsed) return false;

        var changed = false;
        foreach (var reward in rewards.OfType<CardReward>().Where(r => !r.CanReroll))
        {
            reward.CanReroll = true;
            _granted.AddOrUpdate(reward, this);
            reward.AfterGenerated += () => OnGenerated(reward);
            changed = true;
        }
        return changed;
    }

    /// <summary>
    /// Runs inside CardReward.Populate, before the screen shows the new cards.
    /// Also runs on the first roll: only a reroll (_hasBeenRerolled) counts.
    /// </summary>
    private void OnGenerated(CardReward reward)
    {
        if (!CosmoAccess.HasBeenRerolled(reward) || !_granted.TryGetValue(reward, out _)) return;
        _granted.Remove(reward);
        if (WasUsed) return;

        Flash();
        WasUsed = true;
        foreach (var result in CosmoAccess.RewardCards(reward).Where(r => r.Card.IsUpgradable))
        {
            var copy = Owner.RunState.CloneCard(result.Card);
            CardCmd.Upgrade(copy);
            result.ModifyCard(copy, this);
        }
    }

    /// <summary>Once used, the other rewards it gave a Reroll button to lose it (checked when their screen opens).</summary>
    internal bool ShouldRemoveReroll(CardReward reward) => WasUsed && _granted.TryGetValue(reward, out _);
}

[HarmonyLib.HarmonyPatch(typeof(MegaCrit.Sts2.Core.Entities.CardRewardAlternatives.CardRewardAlternative),
    nameof(MegaCrit.Sts2.Core.Entities.CardRewardAlternatives.CardRewardAlternative.Generate))]
public static class LuckyGloveRerollPatch
{
    [HarmonyLib.HarmonyPrefix]
    public static void Prefix(CardReward cardReward)
    {
        if (cardReward.Player.Relics.OfType<LuckyGlove>().Any(g => g.ShouldRemoveReroll(cardReward)))
            cardReward.CanReroll = false;
    }
}
