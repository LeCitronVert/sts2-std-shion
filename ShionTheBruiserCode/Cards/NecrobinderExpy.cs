using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Necrobinder: your partner Summons a copy of your Osty (its current HP) that lasts one turn.
/// If the copy survives the enemy's next turn, you Summon its remaining HP. Exhaust.
/// The copy is tracked by the <see cref="Expy"/> power on the partner.
/// </summary>
public class NecrobinderExpy() : NecrobinderTeamUpCard(2,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("OstyHp").WithMultiplier((card, _) => ((NecrobinderExpy)card).PreviewOstyHp()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.SummonStatic)];

    /// <summary>The Necrobinder: the owner on the original card, the Initiator on the partner's copy.</summary>
    private Player Necrobinder => Initiator ?? Owner;

    /// <summary>Preview only: the HP of the Necrobinder's Osty.</summary>
    private int PreviewOstyHp() => CombatState is not null && Necrobinder.IsOstyAlive ? Necrobinder.Osty!.CurrentHp : 0;

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        if (!initiator.IsOstyAlive) return;

        var result = await OstyCmd.Summon(choiceContext, partner, initiator.Osty!.CurrentHp, this);
        if (result.Amount <= 0 || !partner.IsOstyAlive) return;

        await TeamUpPower.Apply<Expy>(choiceContext, partner.Creature, initiator.Creature, result.Amount, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
