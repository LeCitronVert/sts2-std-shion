using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class FellDragon : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    public const int HpLossCap = 5;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("HpLossCap", HpLossCap)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var uniqueBuffsCount = ShionMechanicsHelper.GetDragonUniqueBuffCount(Owner);
        if (uniqueBuffsCount == 0) return;
        
        Decimal num = (Decimal) 5 * uniqueBuffsCount;
        Decimal hpLoss = Math.Min(2M  * uniqueBuffsCount, HpLossCap);
        _ = (await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Owner, num, Owner, null!))!;
        _ = (await CreatureCmd.GainBlock(Owner, new BlockVar(num, ValueProp.Unpowered), null!))!;
        _ = (await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, new DamageVar(hpLoss , ValueProp.Unblockable), Owner));
    }
    
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return dealer != null && (dealer != this.Owner && !this.Owner.Pets.Contains<Creature>(dealer) || !props.IsPoweredAttack() || cardSource == null || !ShionMechanicsHelper.IsBerserk(dealer)) ? 1M : 1.5M;
    }
    
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner)
        {
            ChangePulse();
        }
        return Task.CompletedTask;
    }
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ChangePulse();
        
        return Task.CompletedTask;
    }

    public void ChangePulse()
    {
        var shouldPulse = ShionMechanicsHelper.IsBerserk(Owner);

        if (shouldPulse)
        {
            StartPulsing();
        }
        else
        {
            StopPulsing();
        }
    }
}