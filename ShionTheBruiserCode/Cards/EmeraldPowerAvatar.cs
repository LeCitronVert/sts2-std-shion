using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Blue Chaos Emerald (Emerald Power): the next Power you play is played 3 (5) additional times.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerAvatar() : EmeraldPowerCard(CardType.Skill, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Blue;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Replays", 3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EmeraldAvatar>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<EmeraldAvatar>(choiceContext, Owner.Creature, DynamicVars["Replays"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Replays"].UpgradeValueBy(2);
    }
}
