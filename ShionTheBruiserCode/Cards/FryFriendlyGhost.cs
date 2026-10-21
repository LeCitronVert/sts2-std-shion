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
/// Cooldown 2. Summon 14 (18).
/// </summary>
public class FryFriendlyGhost() : FryCooldownCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override int CanonicalStarCost => 2;

    protected override int BaseCooldown => 2;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [new SummonVar(14M)];

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await OstyCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Summon.UpgradeValueBy(4M);
    }
}
