using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Add a copy of the first X Slashes you play each turn into your Hand.
/// </summary>
public class ComboMeter : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SeelsSlash>()];

    private int _triggersThisTurn;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !SeelsMechanicsHelper.IsSlash(cardPlay.Card)) return;
        if (_triggersThisTurn >= Amount || Owner.CombatState is null) return;

        _triggersThisTurn++;
        Flash();
        var copy = Owner.CombatState.CreateCard(cardPlay.Card.CanonicalInstance, Owner.Player!);
        if (cardPlay.Card.IsUpgraded) CardCmd.Upgrade(copy);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner.Player));
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side) _triggersThisTurn = 0;
        return Task.CompletedTask;
    }
}
