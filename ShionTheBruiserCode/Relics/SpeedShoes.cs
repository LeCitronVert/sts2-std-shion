using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class SpeedShoes() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;

        if (card.Owner != Owner || !card.Tags.Contains(CardTag.Strike))
        {
            return;
        }
        
        Flash();
        await CardPileCmd.Draw(choiceContext, card.Owner);
    }
}