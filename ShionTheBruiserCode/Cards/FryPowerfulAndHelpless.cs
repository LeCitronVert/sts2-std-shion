using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Shion), Fry: your partner can't Patch Up nor revive this fight, but deals 50% (100%) more Attack damage.
/// Your Requests no longer trigger their effects: they give you 2 (4) Strength and Dexterity when they complete instead.
/// </summary>
public class FryPowerfulAndHelpless() : FryTeamUpCard(1,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Character.ShionTheBruiser>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Percent", PowerfulAndHelpless.DamagePercentPerStack),
        new DynamicVar("Stats", PowerfulAndHelpless.StatsPerStack),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Request),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await TeamUpPower.Apply<PowerfulAndHelpless>(choiceContext, initiator.Creature, partner.Creature, IsUpgraded ? 2 : 1, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Percent"].UpgradeValueBy(PowerfulAndHelpless.DamagePercentPerStack);
        DynamicVars["Stats"].UpgradeValueBy(PowerfulAndHelpless.StatsPerStack);
    }
}
