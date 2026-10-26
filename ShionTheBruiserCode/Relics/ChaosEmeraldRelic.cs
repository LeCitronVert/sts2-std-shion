using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Base class for the Chaos Emerald relics, only obtainable through the Special Stage quest.
/// EventRelicPool never feeds random relic drops, so these can't be found anywhere else.
/// The combo ("Chaos Emeralds") variants share the effect of their solo version; they will unlock Act 4 later.
/// </summary>
[Pool(typeof(EventRelicPool))]
public abstract class ChaosEmeraldRelic : ShionTheBruiserRelic
{
    public abstract ChaosEmeraldColor Color { get; }

    public virtual bool IsCombo => false;

    public override RelicRarity Rarity => RelicRarity.Event;

    private string Variant => IsCombo ? "combo" : "solo";

    public override string PackedIconPath =>
        $"emeralds/emerald_{Variant}_{ChaosEmeraldHelper.ColorKey(Color)}.png".RelicImagePath();

    protected override string PackedIconOutlinePath => $"emeralds/emerald_{Variant}_white.png".RelicImagePath();

    protected override string BigIconPath => PackedIconPath;
}
