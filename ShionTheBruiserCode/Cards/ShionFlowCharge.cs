using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFlowCharge() : ShionTheBruiserCard(1,
    CardType.Power, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<FlowCharge>(3),
        new("VigorAmount", FlowCharge.vigorAmount),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var ownerHasPowerAlready = card.Owner.HasPower<FlowCharge>();
        
        var power = PowerCmd.Apply<FlowCharge>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["FlowCharge"].BaseValue,
            card.Owner.Creature,
            card
        );

        if (!ownerHasPowerAlready)
        {
            var appliedPower = card.Owner.Creature.GetPower<FlowCharge>();
            if (appliedPower != null)
            {
                await PowerCmd.TickDownDuration(appliedPower);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FlowCharge"].UpgradeValueBy(2);
    }
}