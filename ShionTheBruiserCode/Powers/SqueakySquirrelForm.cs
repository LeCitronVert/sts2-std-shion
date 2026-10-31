using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times you play a Power each turn, draw 1 card.
/// </summary>
public class SqueakySquirrelForm : SlalFirstPlaysEachTurnPower
{
    public const int CardsDrawn = 1;

    private const double PouicChance = 0.1;
    private static readonly LocString PouicLine = new("powers", "SHIONTHEBRUISER-SQUEAKY_SQUIRREL_FORM.pouic");

    private bool _hasPouiced;

    protected override bool ShouldTrigger(CardPlay cardPlay) => cardPlay.Card.Type == CardType.Power;

    protected override async Task OnTriggered(PlayerChoiceContext choiceContext, CardPlay cardPlay, Player player)
    {
        await CardPileCmd.Draw(choiceContext, CardsDrawn, player);

        // Purely cosmetic, so it doesn't use the combat RNG.
        if (!_hasPouiced || System.Random.Shared.NextDouble() < PouicChance)
        {
            _hasPouiced = true;
            TalkCmd.Play(PouicLine, Owner, VfxColor.Orange, VfxDuration.Standard);
        }
    }
}
