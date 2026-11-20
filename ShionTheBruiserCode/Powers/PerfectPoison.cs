using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Perfect Poison (Team-Up): your next Amount Attack(s) apply Poison equal to the unblocked damage they deal.
/// </summary>
public class PerfectPoison : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    private CardModel? _poisonedAttack;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (_poisonedAttack is null && cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner.Creature == Owner)
        {
            _poisonedAttack = cardPlay.Card;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (_poisonedAttack is null || cardSource != _poisonedAttack || dealer != Owner) return;
        if (result.UnblockedDamage <= 0 || target.IsDead) return;

        Flash();
        await PowerCmd.Apply<PoisonPower>(choiceContext, target, result.UnblockedDamage, Owner, cardSource);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_poisonedAttack is null || cardPlay.Card != _poisonedAttack) return;
        // Replayed attacks (Double Down...) keep applying Poison until their last play.
        if (cardPlay.PlayIndex < cardPlay.PlayCount - 1) return;

        _poisonedAttack = null;
        await PowerCmd.Decrement(this);
    }
}
