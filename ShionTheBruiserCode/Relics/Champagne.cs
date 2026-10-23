using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Once per turn, after you've played a Common, an Uncommon and a Rare card, gain 1 Slippery (like Rainbow Ring).
/// </summary>
public class Champagne : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SlipperyPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SlipperyPower>(),
    ];

    private bool _commonPlayed;
    private bool _uncommonPlayed;
    private bool _rarePlayed;
    private bool _activatedThisTurn;

    private bool ActivatedThisTurn
    {
        get => _activatedThisTurn;
        set
        {
            AssertMutable();
            _activatedThisTurn = value;
            Status = value ? RelicStatus.Active : RelicStatus.Normal;
        }
    }

    private void Reset()
    {
        AssertMutable();
        _commonPlayed = false;
        _uncommonPlayed = false;
        _rarePlayed = false;
        ActivatedThisTurn = false;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature)) Reset();
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress || ActivatedThisTurn) return;

        AssertMutable();
        _commonPlayed |= cardPlay.Card.Rarity == CardRarity.Common;
        _uncommonPlayed |= cardPlay.Card.Rarity == CardRarity.Uncommon;
        _rarePlayed |= cardPlay.Card.Rarity == CardRarity.Rare;
        if (!_commonPlayed || !_uncommonPlayed || !_rarePlayed) return;

        Flash();
        ActivatedThisTurn = true;
        await PowerCmd.Apply<SlipperyPower>(choiceContext, Owner.Creature, DynamicVars["SlipperyPower"].BaseValue, Owner.Creature, null);
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        Reset();
        return Task.CompletedTask;
    }
}
