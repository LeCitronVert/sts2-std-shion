using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Unthreatening Smile: apply 2 Weak. If you are Weak, apply 2 Vulnerable. Gain 1 (2) Vulnerable.
/// (The class keeps its old name so existing saves still find it.)
/// </summary>
public class SlalUnthreateningLook() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
        new PowerVar<VulnerablePower>(2),
        new DynamicVar("SelfVulnerable", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.HasPower<WeakPower>();

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        if (card.Owner.Creature.HasPower<WeakPower>() && play.Target.IsAlive)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, card.DynamicVars.Vulnerable.BaseValue, card.Owner.Creature, card);
        }

        await PowerCmd.Apply<VulnerablePower>(choiceContext, card.Owner.Creature, card.DynamicVars["SelfVulnerable"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SelfVulnerable"].UpgradeValueBy(1);
    }
}
