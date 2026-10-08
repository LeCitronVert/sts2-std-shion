using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BurlesqueWindUp(): SlalTheEndlessPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner) return;

        if (cardPlay.Card.Type == CardType.Attack)
        {
            await PowerCmd.Remove(this);
            return;
        }
        
        await PowerCmd.Apply<VigorPower>(
            choiceContext, 
            Owner, 
            Amount, 
            Owner,
            null
        );
    }
}