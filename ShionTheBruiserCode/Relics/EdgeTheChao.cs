using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

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
        HoverTipFactory.FromPower<VigorPower>()
    ];

    // Accumulates the vigor to refund until after the attack fully resolves.
    // Applying the refund directly inside AfterPowerAmountChanged would happen
    // while VigorPower.AfterAttack still holds a reference to the spent attack
    // command in its internal Data.commandToModify. Because VigorPower only
    // latches a new attack when commandToModify is null, any subsequent attack
    // would find it non-null and skip vigor application entirely.
    // Deferring to AfterAttack ensures VigorPower has already finished its own
    // AfterAttack (and cleared / consumed that reference) before new vigor lands.
    private decimal _pendingVigorRefund;

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext context,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power.Owner != Owner.Creature || power is not VigorPower || amount >= 0)
            return Task.CompletedTask;

        _pendingVigorRefund += Math.Min(
            Math.Abs(amount) * (RefundPercent / 100M),
            RefundCap
        );

        return Task.CompletedTask;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner.Creature || _pendingVigorRefund <= 0)
            return;

        var refund = _pendingVigorRefund;
        _pendingVigorRefund = 0;

        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            Owner.Creature,
            refund,
            Owner.Creature,
            null);
    }
}