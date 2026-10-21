using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Every 10 Blood you gain, heal 1 HP. The counter is kept between combats (like Nunchaku).
/// </summary>
public class RedFang : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShowCounter => true;

    public override int DisplayAmount => BloodGained % DynamicVars["Blood"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Blood", 10M),
        new HealVar(1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    private int _bloodGained;

    [SavedProperty]
    public int BloodGained
    {
        get => _bloodGained;
        set
        {
            AssertMutable();
            _bloodGained = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not Blood || power.Owner != Owner.Creature || amount <= 0) return;

        var threshold = DynamicVars["Blood"].IntValue;
        var before = BloodGained / threshold;
        BloodGained += (int)amount;
        var heals = BloodGained / threshold - before;
        if (heals <= 0) return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, heals * DynamicVars.Heal.BaseValue);
    }
}
