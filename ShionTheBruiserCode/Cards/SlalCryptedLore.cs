using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 8 damage and double the enemy's Confusion. If it has 20 Confusion or more, Stun it. Exhaust.
/// </summary>
public class SlalCryptedLore() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8M, ValueProp.Move),
        new DynamicVar("StunThreshold", 20),
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ConfusionAmount").WithMultiplier((_, target) => target?.GetPowerAmount<Confusion>() ?? 0),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.Static(StaticHoverTip.Stun),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (!play.Target.IsAlive) return;

        var confusion = play.Target.GetPowerAmount<Confusion>();
        if (confusion > 0)
        {
            await PowerCmd.Apply<Confusion>(choiceContext, play.Target, confusion, card.Owner.Creature, card);
        }

        if (play.Target.IsAlive && play.Target.GetPowerAmount<Confusion>() >= card.DynamicVars["StunThreshold"].BaseValue)
        {
            await CreatureCmd.Stun(play.Target);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
