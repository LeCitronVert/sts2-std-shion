using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Weak. Apply 1 Weak, and Confusion equal to your Block, to ALL enemies. Lose all your Block.
/// </summary>
public class SlalFrontflipAndAHalf() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new DynamicVar("SelfWeak", 1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ConfusionAmount").WithMultiplier((card, _) => card.Owner.Creature.Block),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars["SelfWeak"].BaseValue, card.Owner.Creature, card);
        await SlalMechanicsHelper.ApplyToAllEnemies<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card);

        var block = card.Owner.Creature.Block;
        if (block <= 0) return;

        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, block, card);
        await CreatureCmd.LoseBlock(card.Owner.Creature, block);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
