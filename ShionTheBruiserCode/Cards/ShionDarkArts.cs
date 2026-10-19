using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDarkArts() : ShionBerserkCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(4),
        new PowerVar<DarkArts>(1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (!ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await CreatureCmd.Damage(choiceContext, card.Owner.Creature, card.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
        }
        await PowerCmd.Apply<DarkArts>(choiceContext, card.Owner.Creature, card.DynamicVars["DarkArts"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
    
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || !ShionMechanicsHelper.IsBerserk(this.Owner.Creature)) return Task.CompletedTask;
        this.ReduceCost();
        return Task.CompletedTask;
    }
    
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner.Creature && ShionMechanicsHelper.IsBerserk(creature))
        {
            this.ReduceCost();
        }
        
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this && ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            this.ReduceCost();
        }
        
        return Task.CompletedTask;
    }

    private void ReduceCost() => this.EnergyCost.SetThisTurn(1);
}