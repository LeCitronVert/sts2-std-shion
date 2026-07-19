using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFlowCharge() : ShionBerserkCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<FlowCharge>(3),
        new("VigorAmount", FlowCharge.VigorAmount),
        new ("BerserkAmount", 9)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await PowerCmd.Apply<VigorPower>(
                choiceContext,
                card.Owner.Creature,
                card.DynamicVars["BerserkAmount"].BaseValue,
                card.Owner.Creature,
                card
            );

            return;
        }
        
        var ownerHasPowerAlready = card.Owner.HasPower<FlowCharge>();
        
        await PowerCmd.Apply<FlowCharge>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["FlowCharge"].BaseValue,
            card.Owner.Creature,
            card
        );

        if (!ownerHasPowerAlready)
        {
            var appliedPower = card.Owner.Creature.GetPower<FlowCharge>();
            if (null != appliedPower)
            {
                await appliedPower.ApplyPower();
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FlowCharge"].UpgradeValueBy(2);
        DynamicVars["BerserkAmount"].UpgradeValueBy(6);
    }
}