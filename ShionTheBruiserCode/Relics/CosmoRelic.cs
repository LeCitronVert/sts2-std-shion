using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>Which runs Cosmo can offer a relic in (the sheet's "Always / Solo only / MP only" column).</summary>
public enum CosmoAvailability
{
    Always,
    SoloOnly,
    MultiplayerOnly,
}

/// <summary>
/// A relic Cosmo can offer (Ancients/Cosmo.cs). See <see cref="Ancients.Cosmo"/> for how her three options are picked.
/// </summary>
public interface ICosmoOption
{
    CosmoAvailability CosmoAvailability { get; }

    /// <summary>The sheet's "Cursed Pool": Cosmo offers one cursed relic per visit.</summary>
    bool IsCursedOption { get; }

    /// <summary>Relics that can't be offered together with this one (checked both ways).</summary>
    IEnumerable<Type> IncompatibleOptions => [];

    /// <summary>Extra conditions for this player. Must only read state every client shares.</summary>
    bool IsAllowedAtCosmo(Player player) => true;
}

/// <summary>
/// Base of Cosmo's relics. EventRelicPool never feeds random relic drops, so they only come from her.
/// </summary>
[Pool(typeof(EventRelicPool))]
public abstract class CosmoRelic : ShionTheBruiserRelic, ICosmoOption
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public abstract CosmoAvailability CosmoAvailability { get; }

    public virtual bool IsCursedOption => false;

    public virtual IEnumerable<Type> IncompatibleOptions => [];

    public virtual bool IsAllowedAtCosmo(Player player) => true;
}
