using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The next X times an enemy attacks you, apply Confusion to that enemy equal to the attack's damage (per hit) after 2 turns.
/// </summary>
public class AnchorThrow : SlalTheEndlessPower
{
    public const int TurnsBeforeConfusion = 2;

    public override string CustomPackedIconPath => "anchor_throw_1.png".PowerImagePath();
    public override string CustomBigIconPath => "anchor_throw_1.png".BigPowerImagePath();

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var attacker = command.Attacker;
        if (attacker == null || attacker.Side == Owner.Side || !attacker.IsAlive) return;

        var hitOnOwner = command.Results.SelectMany(r => r).FirstOrDefault(r => r.Receiver == Owner);
        if (hitOnOwner == null || !hitOnOwner.Props.IsPoweredAttack()) return;

        Flash();
        var anchored = await PowerCmd.Apply<AnchoredDown>(choiceContext, attacker, TurnsBeforeConfusion, Owner, null);
        anchored?.SetConfusion(hitOnOwner.TotalDamage);
        await PowerCmd.Decrement(this);
    }
}
