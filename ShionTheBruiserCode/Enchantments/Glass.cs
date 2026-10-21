using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Enchantments;

/// <summary>
/// Glass X: deals X additional damage and applies 1 Vulnerable.
/// </summary>
public class Glass : CustomEnchantmentModel
{
    public const int VulnerableAmount = 1;

    public override bool HasExtraCardText => true;
    public override bool ShowAmount => true;

    protected override string? CustomIconPath => "glass.png".PowerImagePath();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        return props.IsPoweredAttack() ? Amount : 0m;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        var target = cardPlay?.Target;
        if (target is null || !target.IsAlive)
        {
            var enemies = Card.CombatState?.HittableEnemies.ToList() ?? [];
            target = Card.Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        }
        if (target is null) return;

        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, VulnerableAmount, Card.Owner.Creature, Card);
    }
}
