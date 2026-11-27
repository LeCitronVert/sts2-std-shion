using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Purple Chaos Emerald (Emerald Power, Power): gain 1 (2) Intangible. Counter the next (2) hindrance(s). Draw 1 (2) additional card(s) at the start of your turn.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerVision() : EmeraldPowerCard(1, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Purple;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IntangiblePower>(1),
        new DynamicVar("Counters", 1),
        new CardsVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IntangiblePower>(), HoverTipFactory.FromPower<CounterStrategy>()];

    protected override async Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<IntangiblePower>(choiceContext, creature, DynamicVars["IntangiblePower"].BaseValue, creature, this);
        await PowerCmd.Apply<CounterStrategy>(choiceContext, creature, DynamicVars["Counters"].BaseValue, creature, this);
        await PowerCmd.Apply<EmeraldVision>(choiceContext, creature, DynamicVars.Cards.BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IntangiblePower"].UpgradeValueBy(1);
        DynamicVars["Counters"].UpgradeValueBy(1);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
