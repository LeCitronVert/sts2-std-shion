using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Gamble 3. Draw 3 cards. Heal HP equal to the energy cost of the cards drawn this way. Can't be alchemized.
/// </summary>
public class FreshLemonade : SeelsTheRecklessPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Gamble", 3M),
        new CardsVar(3),
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await SeelsMechanicsHelper.Gamble(choiceContext, Owner, DynamicVars["Gamble"].IntValue, this);
        var drawn = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        var heal = drawn.Where(c => !c.EnergyCost.CostsX).Sum(c => c.EnergyCost.GetResolved());
        if (heal > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, heal);
        }
    }
}
