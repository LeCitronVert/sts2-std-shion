using ShionTheBruiser.ShionTheBruiserCode.Visuals;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 8 damage. Apply 2 Weak. Increase this card's damage by 4 each time you gain Flashback this combat
/// (Memory's start of turn gains count too).
/// </summary>
public class SlalFunnyGag() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(8),
        new ExtraDamageVar(4),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => SlalMechanicsHelper.GetTimesFlashbackGained(card.Owner.Creature)),
        new PowerVar<WeakPower>(2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Flashback>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        // A flower pot falls on the target's head; the hit lands when it does
        if (SlalVfx.PlayFallingGag(play.Target, SlalVfx.Gag.FlowerPot))
        {
            await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", 0f);
            await Cmd.Wait(SlalVfx.GagFallTime);
        }

        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card, play)
            .Targeting(play.Target)
            .WithNoAttackerAnim()
            .WithHitFx(null, null, "heavy_attack.mp3")
            .Execute(choiceContext);

        if (play.Target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
