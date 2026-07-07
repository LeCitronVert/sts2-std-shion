using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCodependency() : ShionTheBruiserCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.AnyAlly)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxDamage", Codependency.MaxDamage)
    ];
    
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var applier = play.Card.Owner.Creature;
        var target = play.Target;
        ArgumentNullException.ThrowIfNull(target);
        
        await PowerCmd.Apply<Codependency>(
            choiceContext,
            applier,
            1,
            target,
            card
        );
        await PowerCmd.Apply<Codependency>(
            choiceContext,
            target,
            1,
            applier,
            card
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}