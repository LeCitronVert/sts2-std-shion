using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you reach 10 Random, choose 1 of 3 cards from any character to add into your Hand (X times).
/// It is free to play this turn. Can be skipped.
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
        var options = SlalMechanicsHelper.GetCharacterCardOptions(Owner.Player, includeOwnCharacter: true);
        for (var i = 0; i < Amount; i++)
        {
            var chosen = await SlalMechanicsHelper.ChooseCardToAddToHand(choiceContext, Owner.Player, options, CardsToChooseFrom, false);
            if (chosen != null && !chosen.EnergyCost.CostsX)
            {
                chosen.SetToFreeThisTurn();
            }
        }
    }
}
