using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

using MegaCrit.Sts2.Core.Entities.Players;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you reach 10 Random, choose 1 of 3 cards from any character to add into your Hand (X times).
/// It is free to play this turn. Can be skipped. Genie Form also does it once when played.
/// </summary>
public class GenieForm : SlalTheEndlessPower, ISlalRandomListener
{
    public const int CardsToChooseFrom = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Random>(),
    ];

    public async Task AfterRandomThresholdReached(PlayerChoiceContext choiceContext, Creature gainer)
    {
        if (gainer != Owner || Owner.Player is null || CombatState is null) return;

        Flash();
        await GrantWishes(choiceContext, Owner.Player, Amount);
    }

    /// <summary>
    /// Chooses 1 of 3 cards from any character to add into the Hand, <paramref name="times"/> times. They are free to play this turn.
    /// </summary>
    public static async Task GrantWishes(PlayerChoiceContext choiceContext, Player player, int times)
    {
        var options = SlalMechanicsHelper.GetCharacterCardOptions(player, includeOwnCharacter: true);
        for (var i = 0; i < times; i++)
        {
            // Made free before being added, so Sparrow Form doesn't trigger on it
            await SlalMechanicsHelper.ChooseCardToAddToHand(choiceContext, player, options, CardsToChooseFrom, false, freeThisTurn: true);
        }
    }
}
