using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class SqueakySquirrelForm : SlalOnPowerPlayedPower
{
    private const double PouicChance = 0.1;
    private static readonly LocString PouicLine = new("powers", "SHIONTHEBRUISER-SQUEAKY_SQUIRREL_FORM.pouic");

    private bool _hasPouiced;

    protected override async Task OnPowerPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount)
    {
        if (Owner.Player is null) return;

        var cards = await SlalMechanicsHelper.PutRandomColoredCardsFromDiscardIntoHand(Owner.Player, amount);
        if (cards.Count == 0) return;

        // Purely cosmetic, so it doesn't use the combat RNG.
        if (!_hasPouiced || System.Random.Shared.NextDouble() < PouicChance)
        {
            _hasPouiced = true;
            TalkCmd.Play(PouicLine, Owner, VfxColor.Orange, VfxDuration.Standard);
        }
    }
}
