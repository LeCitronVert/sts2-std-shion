using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(ShionTheBruiserRelicPool))]
public class EdgeTheChao() : ShionTheBruiserRelic
{
    // Shion relic
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public const int RefundPercent = 20;
    public const int RefundCap = 15;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RefundPercent", RefundPercent),
        new DynamicVar("RefundCap", RefundCap)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VigorNextTurn>()
    ];

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power.Owner != Owner.Creature || power is not VigorPower || amount >= 0)
            return;

        // Rounded down per consumption so Vigor stays a whole number.
        var vigorNextTurn = Math.Min(Math.Floor(-amount * RefundPercent / 100M), RefundCap);
        if (vigorNextTurn <= 0)
            return;

        // Applying VigorNextTurn (not VigorPower) mid-attack is safe: it doesn't touch
        // VigorPower's pending attack command, so no deferral to AfterAttack is needed.
        Flash();
        await PowerCmd.Apply<VigorNextTurn>(context, Owner.Creature, vigorNextTurn, Owner.Creature, null);
    }
}