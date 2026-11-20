#if STS2_BETA
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
/// Team-Up (Fool), Fry: costs 0 and X Stars for Fry, 3 energy for the Fool.
/// Your partner Shoots an Arrow for 10 (14) damage X times (X = the Stars Fry spent). Stops when they run out of Arrows.
/// Beta only (the Fool doesn't exist on the main branch).
/// No Shoot tag on purpose: the game would give Fry an Arrow slot just for holding it.
/// </summary>
public class FrySacredBowAndArrows() : FryTeamUpCard(0,
    CardType.Attack,
    TargetType.AnyEnemy)
{
    public const int PartnerCost = 3;

    public override CharacterModel PartnerCharacter => ModelDb.Character<Fool>();

    // X Stars on Fry's card; the Fool's copy costs energy instead.
    public override bool HasStarCostX => !IsPartnerCopy;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10M, ValueProp.Move | ValueProp.FromArrow),
        new DynamicVar("Shots", 0M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArrowHoverTip, HoverTipFactory.FromPower<AimPower>()];

    protected override bool ShouldGlowRedInternal => IsPartnerCopy && !ArrowCmd.PlayerHasAnyArrows(Owner);

    protected override void PrepareCopy(TeamUpCard copy, CardPlay play)
    {
        copy.DynamicVars["Shots"].BaseValue = ResolveStarXValue();
    }

    protected override void OnBecamePartnerCopy()
    {
        EnergyCost.SetThisCombat(PartnerCost);
    }

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        for (var i = 0; i < DynamicVars["Shots"].IntValue; i++)
        {
            var target = FoolTeamUpHelper.TargetOrRandom(partner, play.Target);
            if (target is null) break;
            if (!await FoolTeamUpHelper.ShootArrow(choiceContext, partner, this, play, target, DynamicVars.Damage.BaseValue)) break;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4M);
    }
}
#endif
