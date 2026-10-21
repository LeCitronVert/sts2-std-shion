using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you create a card, shuffle a copy of that card into your Discard Pile.
/// </summary>
public class DrivingLicense : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    // The copy is itself a created card: don't copy it again.
    private bool _isCopying;

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (creator != Owner || _isCopying || card.Pile is null || !card.Pile.Type.IsCombatPile()) return;

        _isCopying = true;
        try
        {
            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Discard, Owner);
        }
        finally
        {
            _isCopying = false;
        }
    }
}
