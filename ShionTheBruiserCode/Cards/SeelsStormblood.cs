using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Ironclad): whenever you or your partner attack a Vulnerable enemy, gain 1 Blood.
/// Your partner's Attacks deal additional damage equal to your Blood.
/// </summary>
public class SeelsStormblood() : SeelsTeamUpCard(3,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Ironclad>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Blood", Stormblood.BloodPerHit)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<Blood>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await TeamUpPower.Apply<Stormblood>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
