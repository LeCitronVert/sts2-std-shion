using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer). Upon pickup, every other player gains 100 Gold.
/// </summary>
public class LemonadeBottles : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(100)];

    public override async Task AfterObtained()
    {
        foreach (var other in Owner.RunState.Players.Where(p => p != Owner))
        {
            await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, other);
        }
    }
}
