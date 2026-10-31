using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// You can't heal or revive during this fight (Patches/ComeWhatMayPatches).
/// At the start of your turn, lose X HP, gain X Energy, X Stars and draw X cards (X = stacks).
/// </summary>
public class ComeWhatMay : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        if (Owner.IsDead) return;
        await PlayerCmd.GainEnergy(Amount, player);
        await PlayerCmd.GainStars(Amount, player);
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
