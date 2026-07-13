using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class ChoiceBand() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public const int VigorAmount = 10;

    private int _attacksUsedThisTurn;

    private int AttacksUsedThisTurn
    {
        get => _attacksUsedThisTurn;
        set
        {
            AssertMutable();
            _attacksUsedThisTurn = value;
            UpdateDisplay();
        }
    }

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => AttacksUsedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("VigorAmount", VigorAmount)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack)
            return Task.CompletedTask;

        AttacksUsedThisTurn++;
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(this.Owner.Creature))
            return;

        var attacksUsed = AttacksUsedThisTurn;
        AttacksUsedThisTurn = 0;

        if (attacksUsed != 1)
            return;

        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            Owner.Creature,
            amount: VigorAmount,
            applier: Owner.Creature,
            cardSource: null);
    }

    public override Task BeforeCombatStart()
    {
        AttacksUsedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        AttacksUsedThisTurn = 0;
        return Task.CompletedTask;
    }

    private void UpdateDisplay()
    {
        this.Status = _attacksUsedThisTurn == 1 ? RelicStatus.Active : RelicStatus.Normal;
        this.InvokeDisplayAmountChanged();
    }
}