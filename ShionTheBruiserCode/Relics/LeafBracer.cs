using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Gain 1 Block for each Star gained.
/// </summary>
public class LeafBracer : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(1M, ValueProp.Unpowered)];

    public override async Task AfterStarsGained(int amount, Player gainer)
    {
        if (gainer != Owner || amount <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, amount * DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
    }
}
