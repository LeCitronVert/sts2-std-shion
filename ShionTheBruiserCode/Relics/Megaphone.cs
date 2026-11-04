using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you Gloat, apply 1 Vulnerable to ALL enemies.
/// </summary>
public class Megaphone : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Gloat>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not Gloat || power.Owner != Owner.Creature || amount <= 0 || Owner.Creature.CombatState is null) return;
        var enemies = Owner.Creature.CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, DynamicVars.Vulnerable.BaseValue, Owner.Creature, null);
    }
}
