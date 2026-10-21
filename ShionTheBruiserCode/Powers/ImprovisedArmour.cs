using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X Request cards you play each turn, gain 10 Block.
/// </summary>
public class ImprovisedArmour : FryTheCommanderPower
{
    public const int Block = 10;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Request)];

    private int _triggersThisTurn;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !cardPlay.Card.Keywords.Contains(FryCardKeywords.Request)) return;
        if (_triggersThisTurn >= Amount) return;

        _triggersThisTurn++;
        Flash();
        await CreatureCmd.GainBlock(Owner, Block, ValueProp.Unpowered, null);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side) _triggersThisTurn = 0;
        return Task.CompletedTask;
    }
}
