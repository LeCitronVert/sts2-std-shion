using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionHammerArm() : ShionBerserkCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Move),
        new PowerVar<StrengthPower>(1),
        new DynamicVar("Duration", HammerArm.Duration),
        new PowerVar<WeakPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var cardSource = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(cardSource.DynamicVars.Damage.BaseValue)
            .FromCard(cardSource)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
        await PowerCmd.Apply<HammerArm>(choiceContext, play.Target, cardSource.DynamicVars["StrengthPower"].BaseValue, cardSource.Owner.Creature, cardSource);
        
        if (ShionMechanicsHelper.TriggerBerserk(this.Owner.Creature))
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, cardSource.DynamicVars["WeakPower"].BaseValue, cardSource.Owner.Creature, cardSource);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
        DynamicVars["WeakPower"].UpgradeValueBy(1);
    }
}