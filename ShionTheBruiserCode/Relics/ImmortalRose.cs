using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. In your next 3 card rewards, a random card is Enchanted with Sown.
/// Same pattern as Silver Crucible: the reward card is swapped for an enchanted copy.
/// </summary>
public class ImmortalRose : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Sown>();

    public override bool ShowCounter => !IsUsedUp;

    public override int DisplayAmount => DynamicVars.Cards.IntValue - RewardsEnchanted;

    public override bool IsUsedUp => RewardsEnchanted >= DynamicVars.Cards.IntValue;

    private int _rewardsEnchanted;

    [SavedProperty]
    public int RewardsEnchanted
    {
        get => _rewardsEnchanted;
        set
        {
            AssertMutable();
            _rewardsEnchanted = value;
            InvokeDisplayAmountChanged();
            if (IsUsedUp) Status = RelicStatus.Disabled;
        }
    }

    public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
    {
        if (player != Owner || IsUsedUp || !options.Flags.HasFlag(CardCreationFlags.IsCardReward)) return false;

        var sown = ModelDb.Enchantment<Sown>();
        var eligible = cardRewards.Where(r => r.Card.Enchantment == null && sown.CanEnchant(r.Card)).ToList();
        if (eligible.Count == 0) return false;

        var chosen = Owner.PlayerRng.Rewards.NextItem(eligible)!;
        var copy = Owner.RunState.CloneCard(chosen.Card);
        CardCmd.Enchant<Sown>(copy, 1m);
        chosen.ModifyCard(copy, this);
        return true;
    }

    public override Task AfterModifyingCardRewardOptions()
    {
        if (IsUsedUp) return Task.CompletedTask;
        Flash();
        RewardsEnchanted++;
        return Task.CompletedTask;
    }
}
