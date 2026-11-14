#if STS2_BETA
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Fool), Seels: your partner Shoots an Arrow dealing 30 damage, with Maim equal to that base damage. Exhaust (upgraded: no Exhaust).
/// Beta only (the Fool doesn't exist on the main branch).
/// Seels applies the Maim and gains its Blood (<see cref="IMaimCredited"/>), even though the Fool shoots.
/// No Shoot tag on purpose: the game would give Seels an Arrow slot just for holding it.
/// </summary>
public class SeelsCruelShot() : SeelsTeamUpCard(3,
    CardType.Attack,
    TargetType.AnyEnemy), IMaimCredited
{
    public const int BaseDamage = 30;

    public override CharacterModel PartnerCharacter => ModelDb.Character<Fool>();

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust, SeelsCardKeywords.Maim];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(BaseDamage, ValueProp.Move | ValueProp.FromArrow),
        // Maims for as much as the Arrow's base damage
        MaimHelper.Var(BaseDamage),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArrowHoverTip, HoverTipFactory.FromPower<AimPower>()];

    // On the Fool's copy, the Initiator (Seels). On Seels' own card, Seels already owns it.
    public Player? MaimCredit => Initiator;

    protected override bool ShouldGlowRedInternal => IsPartnerCopy && !ArrowCmd.PlayerHasAnyArrows(Owner);

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await FoolTeamUpHelper.ShootArrow(choiceContext, partner, this, play, play.Target, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
#endif
