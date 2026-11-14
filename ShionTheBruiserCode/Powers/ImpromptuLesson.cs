#if STS2_BETA
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Impromptu Lesson (Team-Up), on the Fool. Each effect happens once per stack:
/// <list type="bullet">
/// <item>Whenever your Partner plays a Strike, they also Shoot an Arrow at its target (or a random enemy) for its base damage.
/// They get an Arrow slot if they have none.</item>
/// <item>Whenever your Partner plays a Defend, they gain Aim equal to its base Block.</item>
/// <item>Whenever your Partner's Arrow deals unblocked damage, gain that much Aim.</item>
/// </list>
/// Beta only.
/// </summary>
public class ImpromptuLesson : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<AimPower>()];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (Owner.IsDead || !Partners.Contains(card.Owner.Creature) || card.Owner.Creature.IsDead) return;
        var partner = card.Owner;

        if (card.Tags.Contains(CardTag.Strike) && FoolTeamUpHelper.GetBaseDamage(card, cardPlay.Target) is > 0 and var damage)
        {
            FoolTeamUpHelper.EnsureArrowSlot(partner);
            for (var i = 0; i < Amount; i++)
            {
                var target = FoolTeamUpHelper.TargetOrRandom(partner, cardPlay.Target);
                if (target == null) break;
                Flash();
                if (!await FoolTeamUpHelper.ShootArrow(choiceContext, partner, card, cardPlay, target, damage)) break;
            }
        }

        if (card.Tags.Contains(CardTag.Defend) && FoolTeamUpHelper.GetBaseBlock(card) is > 0 and var block)
        {
            Flash();
            await PowerCmd.Apply<AimPower>(choiceContext, partner.Creature, block * Amount, Owner, null);
        }
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (Owner.IsDead || dealer == null || !Partners.Contains(dealer) || !props.HasFlag(ValueProp.FromArrow)) return;
        if (result.UnblockedDamage <= 0) return;

        Flash();
        await PowerCmd.Apply<AimPower>(choiceContext, Owner, result.UnblockedDamage * Amount, Owner, null);
    }
}
#endif
