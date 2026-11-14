#if STS2_BETA
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Fool: your partner's Strikes also Shoot an Arrow for their base damage, and their Defends give them
/// Aim equal to their base Block. Gain Aim equal to the unblocked damage of your partner's Arrows.
/// Beta only (the Fool doesn't exist on the main branch).
/// </summary>
public class FoolImpromptuLesson() : FoolTeamUpCard(2,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArrowHoverTip, HoverTipFactory.FromPower<AimPower>()];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        FoolTeamUpHelper.EnsureArrowSlot(partner);
        if (initiator.Creature.IsDead) return;
        await TeamUpPower.Apply<ImpromptuLesson>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
#endif
