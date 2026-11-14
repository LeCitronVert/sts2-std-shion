#if STS2_BETA
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Token, given by Arrow Rain. X-cost: give 10X (upgraded: 12X) Aim to the player who used Arrow Rain. Exhaust.
/// Beta only (the Fool doesn't exist on the main branch).
/// </summary>
[Pool(typeof(TokenCardPool))]
public class FoolAimAssist() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("AimPerX", 10),
        new StringVar("Shooter"),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("AimAmount").WithMultiplier((card, _) =>
            PreviewHelper.XValue(card) * card.DynamicVars["AimPerX"].BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.FromPower<AimPower>(),
    ];

    private Player? _shooter;

    /// <summary>The player who used Arrow Rain. A copy made some other way falls back to a living Fool ally.</summary>
    private Player? Shooter => _shooter ?? CombatState?.GetTeammatesOf(Owner.Creature)
        .FirstOrDefault(c => c is { IsPlayer: true, IsAlive: true } && c.Player!.Character is Fool)?.Player;

    public void SetShooter(Player player)
    {
        _shooter = player;
        ((StringVar)DynamicVars["Shooter"]).StringValue = SlalMechanicsHelper.GetPlayerName(player);
    }

    protected override bool IsPlayable => Shooter is { Creature.IsAlive: true };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = ResolveEnergyXValue();
        if (x <= 0 || Shooter is not { Creature.IsAlive: true } shooter) return;

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<AimPower>(choiceContext, shooter.Creature, x * DynamicVars["AimPerX"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AimPerX"].UpgradeValueBy(2);
    }
}
#endif
