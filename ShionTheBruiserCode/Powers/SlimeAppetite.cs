using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Slimy card, apply X Weak to a random enemy and gain X Plating.
/// </summary>
public class SlimeAppetite : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !SlalMechanicsHelper.IsSlimy(cardPlay.Card) || CombatState is null) return;

        Flash();
        var enemy = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy != null)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, Amount, Owner, null);
        }
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
