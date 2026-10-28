using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Eccentrics (Team-Up), on the Initiator: whenever you gain Random, your Partner gains as much (times Amount).
/// Whenever your Partner's Random reaches 10, gain Amount Strength and Dexterity.
/// </summary>
public class Eccentrics : TeamUpPower, ISlalRandomListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Random>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public async Task AfterRandomGained(PlayerChoiceContext choiceContext, Creature gainer, decimal amount)
    {
        if (gainer != Owner) return;

        foreach (var partner in LivingPartners.ToList())
        {
            Flash();
            await SlalMechanicsHelper.GainRandom(choiceContext, partner, amount * Amount);
        }
    }

    public async Task AfterRandomThresholdReached(PlayerChoiceContext choiceContext, Creature gainer)
    {
        if (!Partners.Contains(gainer) || Owner.IsDead) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
