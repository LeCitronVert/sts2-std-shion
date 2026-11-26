using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Grey Chaos Emerald (Emerald Power): gain 2 (3) Free Attack, Free Skill and Free Power.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerExtra() : EmeraldPowerCard(CardType.Skill, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("FreeCards", 2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FreeAttackPower>(),
        HoverTipFactory.FromPower<FreeSkillPower>(),
        HoverTipFactory.FromPower<FreePowerPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var amount = DynamicVars["FreeCards"].BaseValue;
        await PowerCmd.Apply<FreeAttackPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
        await PowerCmd.Apply<FreeSkillPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
        await PowerCmd.Apply<FreePowerPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FreeCards"].UpgradeValueBy(1);
    }
}
