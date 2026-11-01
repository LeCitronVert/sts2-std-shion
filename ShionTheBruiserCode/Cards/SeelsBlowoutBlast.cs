using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Shion). Costs 0 for Seels and X for the Partner.
/// Seels Blood Bends X; the Partner deals 14 damage as many times as Seels' X plus their own X. Exhaust.
/// Upgraded: Blood Bend X+1 and Partner X+1.
/// </summary>
public class SeelsBlowoutBlast() : SeelsTeamUpCard(0,
    CardType.Attack,
    TargetType.AnyEnemy)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Character.ShionTheBruiser>();

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust, SeelsCardKeywords.BloodBend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14M, ValueProp.Move),
        new DynamicVar("BloodBendBonus", 0),
        new DynamicVar("XBonus", 0),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((card, _) => ((SeelsBlowoutBlast)card).PreviewHits()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
    ];

    // The Partner pays X energy for their copy.
    protected override void OnBecamePartnerCopy()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, true));
    }

    /// <summary>
    /// Preview only. Seels' card shows their side (Blood Bend X); the Partner's copy shows the total.
    /// </summary>
    private int PreviewHits()
    {
        if (CombatState is null) return 0;
        if (Initiator is not { } initiator)
        {
            return SeelsMechanicsHelper.GetBlood(Owner.Creature) + DynamicVars["BloodBendBonus"].IntValue;
        }

        return SeelsMechanicsHelper.GetBlood(initiator.Creature) + DynamicVars["BloodBendBonus"].IntValue
            + PreviewHelper.XValue(this) + DynamicVars["XBonus"].IntValue;
    }

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var partnerX = ResolveEnergyXValue() + DynamicVars["XBonus"].IntValue;
        var bloodX = await SeelsMechanicsHelper.BloodBend(choiceContext, initiator, this) + DynamicVars["BloodBendBonus"].IntValue;
        var hits = bloodX + partnerX;
        if (hits <= 0) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BloodBendBonus"].UpgradeValueBy(1);
        DynamicVars["XBonus"].UpgradeValueBy(1);
    }
}
