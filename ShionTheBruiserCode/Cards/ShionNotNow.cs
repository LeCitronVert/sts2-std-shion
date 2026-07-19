using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionNotNow() : ShionBerserkCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar("BerserkEnergy", 1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        CardKeyword.Retain,
    ];
    
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await PlayerCmd.GainEnergy(DynamicVars["BerserkEnergy"].IntValue, card.Owner);
            return;
        }
        
        var amountOfEnergyNextTurn = card.ResolveEnergyXValue();
        if (amountOfEnergyNextTurn == 0) return;
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, card.Owner.Creature, amountOfEnergyNextTurn, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}