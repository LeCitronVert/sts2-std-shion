using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// When played and at the start of your turn, apply Confusion equal to the number of times you gained Random this combat (times X) to ALL enemies.
/// Doesn't stack: each play adds another instance, like The Bomb.
/// The displayed amount is the Confusion that will be applied.
/// </summary>
public class Conclusion : SlalTheEndlessPower, ISlalRandomListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => GetConfusionAmount();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<Random>(),
    ];

    private int GetConfusionAmount() => SlalMechanicsHelper.GetTimesRandomGained(Owner) * Amount;

    public Task AfterRandomGained(PlayerChoiceContext choiceContext, Creature gainer, decimal amount)
    {
        if (gainer == Owner) InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        await Trigger(choiceContext);
    }

    /// <summary>
    /// Applies the Confusion. Also called by the card when it's played.
    /// </summary>
    public async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (CombatState is null) return;

        var amount = GetConfusionAmount();
        if (amount <= 0) return;

        Flash();
        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, Owner, amount, null);
    }
}
