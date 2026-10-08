using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalDistraction(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1)
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