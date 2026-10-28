using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Ironclad): when you and your partner attack Vulnerable enemies, your attacks gain 50% Critical Hit chance.
/// This amount is doubled if you are Berserk. Ironclad gains the crit and Berserk trackers.
/// </summary>
public class ShionHyperOffense() : ShionTeamUpCard(3,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Ironclad>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<HyperOffense>(50),
        new CritDisplayTooltipVar(),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        foreach (var creature in new[] { initiator.Creature, partner.Creature })
        {
            await ShionMechanicsHelper.EnsureCreatureHasCritTracker(creature);
            await ShionMechanicsHelper.EnsureCreatureHasBerserkIndicator(creature);
            await PowerCmd.Apply<HyperOffense>(choiceContext, creature, DynamicVars["HyperOffense"].BaseValue, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
