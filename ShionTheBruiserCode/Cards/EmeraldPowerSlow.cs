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
/// Yellow Chaos Emerald Burst (Emerald Power, 1-cost Power): this (2) turn(s), gain as many Rings as the damage you deal.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerSlow() : EmeraldPowerCard(1, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Turns", 1M)];

    protected override async Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<EmeraldSlowBurst>(choiceContext, creature, DynamicVars["Turns"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Turns"].UpgradeValueBy(1M);
    }
}
