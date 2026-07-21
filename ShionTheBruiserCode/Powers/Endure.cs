using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Endure() : CritContributorPower, ICritHitRateContributor
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(50),
        new DynamicVar("DebuffAmount", 5),
        new CritDisplayTooltipVar(),
        new CritHitRateVar(0.33M)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
    
    public override bool ShouldDieLate(Creature creature)
    {
        return creature != this.Owner;
    }
    
    public override async Task AfterPreventingDeath(Creature creature)
    {
        var power = this;
        power.Flash();
        await CreatureCmd.Heal(creature, Math.Max(1M, (Decimal) creature.MaxHp * (power.DynamicVars.Heal.BaseValue / 100M)));
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, -DynamicVars["DebuffAmount"].BaseValue, Owner, null!);
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner, -DynamicVars["DebuffAmount"].BaseValue, Owner, null!);
        await PowerCmd.Decrement(power);
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

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard == null || sourceCard.Owner.Creature == Owner)
            return ShionMechanicsHelper.IsBerserk(Owner) ? DynamicVars[CritHitRateVar.Key].BaseValue : 0M;
        
        return 0;    }
}