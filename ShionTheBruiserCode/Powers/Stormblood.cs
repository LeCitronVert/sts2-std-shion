using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Stormblood (Team-Up), on Seels: whenever you or your Partner hit a Vulnerable enemy with an attack, gain 1 Blood (times Amount).
/// Your Partner's Attacks deal additional damage equal to your Blood (times Amount).
/// </summary>
public class Stormblood : TeamUpPower
{
    public const int BloodPerHit = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Blood", BloodPerHit)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<Blood>(),
    ];

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == null || !Partners.Contains(dealer) || Owner.IsDead) return 0m;
        if (!props.IsPoweredAttack() || cardSource is not { Type: CardType.Attack }) return 0m;
        return SeelsMechanicsHelper.GetBlood(Owner) * Amount;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == null || (dealer != Owner && !Partners.Contains(dealer)) || Owner.IsDead) return;
        if (!props.IsPoweredAttack() || !target.HasPower<VulnerablePower>()) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner, BloodPerHit * Amount);
    }
}
