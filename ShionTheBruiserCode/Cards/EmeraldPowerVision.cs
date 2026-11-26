using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Purple Chaos Emerald (Emerald Power): gain 1 (2) Intangible. Counter the next 2 (4) hindrances.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerVision() : EmeraldPowerCard(CardType.Power, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Purple;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IntangiblePower>(1),
        new DynamicVar("Counters", 2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.FromPower<CounterStrategy>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature,
            DynamicVars["IntangiblePower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<CounterStrategy>(choiceContext, Owner.Creature,
            DynamicVars["Counters"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IntangiblePower"].UpgradeValueBy(1);
        DynamicVars["Counters"].UpgradeValueBy(2);
    }
}
