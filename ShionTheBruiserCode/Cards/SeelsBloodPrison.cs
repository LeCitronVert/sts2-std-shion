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
/// Blood Bend X. Deal 6 damage to ALL enemies. Apply X (X+1) Vulnerable to ALL enemies (twice).
/// </summary>
public class SeelsBloodPrison() : SeelsTheRecklessCard(0,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AllEnemies)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6M, ValueProp.Move),
        new DynamicVar("Bonus", 0M),
        new RepeatVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this) + DynamicVars["Bonus"].IntValue;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);

        if (x <= 0) return;
        for (var i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState.HittableEnemies.ToList(), x, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1M);
        DynamicVars.Repeat.UpgradeValueBy(1M);
    }
}
