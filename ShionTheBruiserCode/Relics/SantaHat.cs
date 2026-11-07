using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. The first Curse that would be added to your Deck is not.
/// </summary>
public class SantaHat : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    private bool _wasUsed;

    public override bool IsUsedUp => _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (_wasUsed) Status = RelicStatus.Disabled;
        }
    }

    public override bool ShouldAddToDeck(CardModel card) =>
        WasUsed || card.Owner != Owner || card.Type != CardType.Curse;

    public override Task AfterAddToDeckPrevented(CardModel card)
    {
        Flash();
        WasUsed = true;
        return Task.CompletedTask;
    }
}
