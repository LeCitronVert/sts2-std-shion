using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class FellDragon() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        var uniqueBuffsCount = ShionMechanicsHelper.GetDragonUniqueBuffCount(Owner);
        if (uniqueBuffsCount == 0) return;
        
        Decimal num = (Decimal) 5 * uniqueBuffsCount;
        Decimal hpLoss = 2M;
        var vigorPower = (await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Owner, num, Owner, (CardModel) null!))!;
        var block = (await CreatureCmd.GainBlock(Owner, new BlockVar(num, ValueProp.Move), (CardPlay) null!))!;
        var selfDamage = (await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, new DamageVar(hpLoss, ValueProp.Unpowered), (CardModel) null!))!;
    }
}