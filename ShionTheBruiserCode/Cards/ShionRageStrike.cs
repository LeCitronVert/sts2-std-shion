using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionRageStrike() : ShionBerserkCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy), ICritHitRateContributor
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(20M),
        new ExtraDamageVar(10),
        // Damage is doubled if owner has 3+ unique buffs
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((model, _) => ShionMechanicsHelper.IsBerserk(model.Owner.Creature) ? 1 : 0),
        new HpLossVar(5),
        new CritDisplayTooltipVar(),
        new CritHitRateVar(40),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this
        );
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .WithValueProp(ValueProp.Move)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(6);
        DynamicVars.ExtraDamage.UpgradeValueBy(5);
        DynamicVars[CritHitRateVar.Key].UpgradeValueBy(20);
    }
    
    public decimal GetCritHitRateBonus(CardModel? sourceCard) =>
        sourceCard == this ? DynamicVars[CritHitRateVar.Key].BaseValue / 100 : 0m;
    
    public override Task BeforeCombatStart()    
    {
        ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
        ShionMechanicsHelper.EnsureCreatureHasCritTracker(Owner.Creature);
        
        return Task.CompletedTask;
    }
}