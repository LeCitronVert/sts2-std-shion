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
        new DamageVar(16, ValueProp.Move),
        new HpLossVar(3),
        new CritDisplayTooltipVar(),
        new CritHitRateVar(40),
        new PatchUpVar(3),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await ShionMechanicsHelper.PatchUpCreature(card.DynamicVars[PatchUpVar.Key].IntValue, card.Owner.Creature);
        }
        else
        {
            await CreatureCmd.Damage(
                choiceContext,
                card.Owner.Creature,
                card.DynamicVars.HpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this
            );
        }
        
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithValueProp(ValueProp.Move)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars[CritHitRateVar.Key].UpgradeValueBy(20);
    }
    
    public decimal GetCritHitRateBonus(CardModel? sourceCard) =>
        sourceCard == this ? DynamicVars[CritHitRateVar.Key].BaseValue / 100 : 0m;
    
    public override async Task BeforeCombatStart()    
    {
        await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(Owner.Creature);
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(Owner.Creature);
    }
}