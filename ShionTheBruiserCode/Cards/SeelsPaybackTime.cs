using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team Blast. Blood Bend X. Deal damage equal to the missing HP of ALL other players (ALL players) X times.
/// </summary>
public class SeelsPaybackTime() : SeelsTheRecklessCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.TeamBlast];

    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (CombatState is null) return;

        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        var damage = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(c => c.IsPlayer && (IsUpgraded || c != Owner.Creature))
            .Sum(c => c.MaxHp - c.CurrentHp);

        if (x > 0 && damage > 0)
        {
            await DamageCmd.Attack(damage)
                .FromCard(this)
                .Targeting(play.Target)
                .WithHitCount(x)
                .WithHitFx("vfx/vfx_bloody_impact")
                .Execute(choiceContext);
        }

        await DreamersMechanicsHelper.ExhaustAllTeamBlasts(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
    }
}
