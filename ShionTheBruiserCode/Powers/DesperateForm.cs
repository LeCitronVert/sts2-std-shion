using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever a Status card is exhausted, gain X Strength, then take damage and gain Vigor equal to your Strength.
/// </summary>
public class DesperateForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card.Owner != Owner.Player || card.Type != CardType.Status) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);

        var strength = Owner.GetPowerAmount<StrengthPower>();
        if (strength > 0)
        {
            await CreatureCmd.Damage(choiceContext, Owner, strength, ValueProp.Unpowered, Owner, null);
            if (Owner.IsAlive)
            {
                await PowerCmd.Apply<VigorPower>(choiceContext, Owner, strength, Owner, null);
            }
        }
    }
}
