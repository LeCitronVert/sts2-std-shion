using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class FellDragon : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var uniqueBuffsCount = ShionMechanicsHelper.GetDragonUniqueBuffCount(Owner);
        if (uniqueBuffsCount == 0) return;
        
        Decimal num = (Decimal) 5 * uniqueBuffsCount;
        Decimal hpLoss = 2M  * uniqueBuffsCount;
        _ = (await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Owner, num, Owner, null!))!;
        _ = (await CreatureCmd.GainBlock(Owner, new BlockVar(num, ValueProp.Unpowered), null!))!;
        _ = (await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, new DamageVar(hpLoss , ValueProp.Unblockable), Owner));
    }
}