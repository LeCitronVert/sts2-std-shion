using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Gain 1 energy. Gain Random equal to the number of cards in your Hand.
/// </summary>
public class CottonCandy : SlalTheEndlessPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Random>(),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await SlalMechanicsHelper.GainRandom(choiceContext, Owner.Creature, PileType.Hand.GetPile(Owner).Cards.Count);
    }
}
