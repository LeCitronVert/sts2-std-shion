using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalDistraction(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Counts the Weak applied first
        new CalculatedVar("DexterityAmount").WithMultiplier((card, target) =>
            target is null ? 0 : target.GetPowerAmount<WeakPower>() + card.DynamicVars.Weak.BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];
    
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, 
            play.Target, 
            DynamicVars.Weak.BaseValue, 
            card.Owner.Creature, 
            card
        );

        var weakAmount = play.Target.GetPowerAmount<WeakPower>();
        if (weakAmount > 0)
        {
            await PowerCmd.Apply<DexterityPower>(
                choiceContext, 
                card.Owner.Creature, 
                weakAmount, 
                card.Owner.Creature, 
                card
            );
            // Takes the Dexterity back at the end of the turn if you are not Weak
            await PowerCmd.Apply<Distraction>(
                choiceContext, 
                card.Owner.Creature, 
                weakAmount, 
                card.Owner.Creature, 
                card
            );
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}