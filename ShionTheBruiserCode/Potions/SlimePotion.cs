using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Transform any number of cards in your Hand into Slimed. Gain 6 Free Status.
/// </summary>
public class SlimePotion : SlalTheEndlessPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<FreeStatus>(6),
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<FreeStatus>(),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var transformable = PileType.Hand.GetPile(Owner).Cards.Count(c => c.IsTransformable);
        var originals = transformable == 0
            ? []
            : (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, transformable),
                c => c.IsTransformable,
                this
            )).ToList();

        if (Owner.Creature.CombatState is null) return;
        foreach (var original in originals)
        {
            await CardCmd.Transform(original, Owner.Creature.CombatState.CreateCard<Slimed>(Owner));
        }

        await PowerCmd.Apply<FreeStatus>(choiceContext, Owner.Creature, DynamicVars["FreeStatus"].BaseValue, Owner.Creature, null);
    }
}
