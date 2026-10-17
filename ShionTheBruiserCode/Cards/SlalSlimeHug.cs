using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalSlimeHug(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public const int NormalMultiplier = 2;
    public const int UpgradedMultiplier = 3;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["WeakPower"].BaseValue,
            Owner.Creature,
            null
        );
        
        var amountOfWeak = play.Target.GetPowerAmount<WeakPower>();
        if (amountOfWeak == 0) return;
        
        var amountToApply = amountOfWeak * (IsUpgraded ? UpgradedMultiplier : NormalMultiplier) - amountOfWeak;
        
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            play.Target,
            amountToApply,
            Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
    }
}