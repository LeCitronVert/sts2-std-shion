using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// At the start of your turn, lose 2 HP and gain Block equal to the number of your non-Exhausted Wounds.
/// Each stack triggers separately (so each one makes you lose HP individually).
/// </summary>
public class LiveWithThePain : SeelsTheRecklessPower
{
    public const int HpLoss = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Wound>()];

    protected virtual bool CountsExhaustedWounds => false;

    private int CountWounds()
    {
        var player = Owner.Player!;
        var cards = CountsExhaustedWounds
            ? SeelsMechanicsHelper.GetAllCombatCards(player)
            : SeelsMechanicsHelper.GetAllCombatCards(player).Where(c => c.Pile?.Type != PileType.Exhaust);
        return cards.Count(SeelsMechanicsHelper.IsWound);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        for (var i = 0; i < Amount; i++)
        {
            Flash();
            await SeelsMechanicsHelper.LoseHp(choiceContext, Owner, HpLoss, null);
            var block = CountWounds();
            if (block > 0)
            {
                await CreatureCmd.GainBlock(Owner, block, ValueProp.Unpowered, null);
            }
        }
    }
}
