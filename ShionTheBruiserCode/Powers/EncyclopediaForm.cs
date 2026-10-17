using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Add a copy of the first X cards you play each turn on top of your Draw Pile. Their energy cost is increased by 1.
/// </summary>
public class EncyclopediaForm : SlalTheEndlessPower
{
    public const int CostIncrease = 1;

    private class Data
    {
        public int procsThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner)) GetInternalData<Data>().procsThisTurn = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !cardPlay.IsFirstInSeries) return;

        var data = GetInternalData<Data>();
        if (data.procsThisTurn >= Amount) return;
        data.procsThisTurn++;

        Flash();
        var copy = cardPlay.Card.CreateClone();
        if (!copy.EnergyCost.CostsX)
        {
            copy.EnergyCost.AddThisCombat(CostIncrease);
        }
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Draw, Owner.Player, CardPilePosition.Top));
    }
}
