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
/// Red Chaos Emerald (Emerald Power): this turn, deal double (triple) Attack damage.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerBullet() : EmeraldPowerCard(CardType.Skill, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Red;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Multiplier", 2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EmeraldBullet>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<EmeraldBullet>(choiceContext, Owner.Creature, DynamicVars["Multiplier"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1);
    }
}
