using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cooldown 3. Channel 1 Dark.
/// </summary>
public class FryCastingAShadow() : FryCooldownCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override int BaseCooldown => 3;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<DarkOrb>()];

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await OrbCmd.Channel<DarkOrb>(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[CooldownKey].UpgradeValueBy(-1M); //todo : upgrade not specified
    }
}
