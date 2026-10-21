using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDefensiveStrategies() : ShionTheBruiserCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<DefensiveStrategies>(1),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Defensive),
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromPower<MightyGuard>(),
        HoverTipFactory.FromPower<CounterStance>()
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        await PowerCmd.Apply<CounterStance>(
            choiceContext,
            card.Owner.Creature,
            999,
            card.Owner.Creature,
            card
        );

        await PowerCmd.Apply<DefensiveStrategies>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["DefensiveStrategies"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DefensiveStrategies"].UpgradeValueBy(1);
    }
}