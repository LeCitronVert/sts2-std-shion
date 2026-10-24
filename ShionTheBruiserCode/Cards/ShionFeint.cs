using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFeint() : CritContributorCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy), IOnCriticalHitListener, IOnCriticalFailListener, ICritHitRateContributor
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new CritDisplayTooltipVar(),
        new CritHitRateVar(33),
        new PowerVar<VigorPower>(10),
        new PowerVar<StrengthPower>(1),
        new PowerVar<WeakPower>(2)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VigorNextTurn>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(Owner.Creature);
        var card = this;
        var target = play.Target;
        ArgumentNullException.ThrowIfNull(target);
        
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(5);
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
        DynamicVars["WeakPower"].UpgradeValueBy(1);
    }

    public async Task OnCriticalHit(Creature attacker, Creature target, DamageResult damageResult, ValueProp props,
        CardModel? sourceCard, PlayerChoiceContext? choiceContext)
    {
        if (sourceCard != this) return;
        
        // Mid-attack, so it goes through VigorNextTurn: adding VigorPower while it's
        // tracking an attack leaves it stuck and never applied again.
        await PowerCmd.Apply<VigorNextTurn>(
                choiceContext ?? new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                DynamicVars["VigorPower"].BaseValue,
                Owner.Creature,
                this)
            ;
            
        await PowerCmd.Apply<StrengthPower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                DynamicVars["StrengthPower"].BaseValue,
                Owner.Creature,
                this)
            ;
    }

    public async Task OnCriticalFail(Creature attacker, Creature target, DamageResult damageResult, ValueProp props,
        CardModel? sourceCard, PlayerChoiceContext? choiceContext)
    {
        if (sourceCard == this)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                DynamicVars["WeakPower"].BaseValue,
                Owner.Creature,
                this)
            ;
        }
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard)=>
        sourceCard == this ? DynamicVars[CritHitRateVar.Key].BaseValue / 100 : 0m;
}