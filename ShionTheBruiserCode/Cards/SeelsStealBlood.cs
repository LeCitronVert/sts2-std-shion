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
/// Lose 3 (2) HP. Deal 9 (11) damage. Whenever this enemy loses HP this turn, gain 1 Blood.
/// </summary>
public class SeelsStealBlood() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(3M),
        new DamageVar(9M, ValueProp.Move),
        new PowerVar<StealBlood>(1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);

        if (play.Target.IsAlive)
        {
            await PowerCmd.Apply<StealBlood>(choiceContext, play.Target, DynamicVars["StealBlood"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-1M);
        DynamicVars.Damage.UpgradeValueBy(2M);
    }
}
