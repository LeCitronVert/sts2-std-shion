using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionPout() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyAlly)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override bool HasEnergyCostX => true;
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var xAmount = card.ResolveEnergyXValue() + 1;
        ArgumentNullException.ThrowIfNull(play.Target?.Player);
        
        await PlayerCmd.GainEnergy(xAmount, play.Target.Player);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}