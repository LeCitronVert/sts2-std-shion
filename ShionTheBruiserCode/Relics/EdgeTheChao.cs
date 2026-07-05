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

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(ShionTheBruiserRelicPool))]
public class EdgeTheChao() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public const int RefundPercent = 20;
    public const int RefundCap = 15;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RefundPercent", RefundPercent),
        new DynamicVar("RefundCap", RefundCap)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];


    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power.Owner != Owner.Creature || power is not VigorPower || amount >= 0) return;

        var amountToRefund = Math.Min(
            Math.Abs(amount) * (RefundPercent / 100M),
            RefundCap
        );
        
        await PowerCmd.Apply<VigorPower>(
            context,
            Owner.Creature,
            amountToRefund,
            Owner.Creature,
            null);
    }
}