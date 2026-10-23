using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Your Attacks apply X Confusion. If the target doesn't lose HP, apply double that amount.
/// Once per Attack and per target, so multi-hit Attacks apply it once.
/// </summary>
public class PirateParrotForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
    ];

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner || command.ModelSource is not CardModel { Type: CardType.Attack } card) return;

        var resultsByTarget = command.Results
            .SelectMany(r => r)
            .Where(r => r.Receiver.Side != Owner.Side)
            .GroupBy(r => r.Receiver)
            .ToList();

        foreach (var targetResults in resultsByTarget)
        {
            var target = targetResults.Key;
            if (!target.IsAlive) continue;

            var lostHp = targetResults.Any(r => r.UnblockedDamage > 0);
            Flash();
            await PowerCmd.Apply<Confusion>(choiceContext, target, lostHp ? Amount : Amount * 2, Owner, card);
        }
    }
}
