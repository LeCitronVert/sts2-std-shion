using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Codependency() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    public const int MaxDamage = 50;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StringVar("Partner"),
        new DynamicVar("MaxDamage", MaxDamage)
    ];
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return dealer != this.Owner || !props.IsPoweredAttack() || dealer == this.Applier ? 1M : GetDamageBoost();
    }
    
    public decimal GetDamageBoost()
    {
        var partner = Applier;
        if (partner == null)
        {
            return 1M;
        }
        
        var missingHpRatio = (partner.MaxHp - partner.CurrentHp)
                             / (float) partner.MaxHp;

        return ((decimal) (missingHpRatio * (float) (MaxDamage * 0.01M))) + 1;
    }
    
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ((StringVar) this.DynamicVars["Partner"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, this.Applier.Player.NetId);
        return Task.CompletedTask;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Applier) return;

        await CreatureCmd.Kill(Owner);
    }
}