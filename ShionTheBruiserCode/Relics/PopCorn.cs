using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (multiplayer). Upon pickup, every other player gains 5 Max HP.
/// </summary>
public class PopCorn : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.MultiplayerOnly;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(5m)];

    public override async Task AfterObtained()
    {
        foreach (var other in Owner.RunState.Players.Where(p => p != Owner))
        {
            await CreatureCmd.GainMaxHp(other.Creature, DynamicVars.MaxHp.BaseValue);
        }
    }
}
