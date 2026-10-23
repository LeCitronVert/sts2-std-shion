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

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

/// <summary>
/// Transform a card in your Hand into Slimed. Slimed are free to play this turn.
/// </summary>
public class SlimePotion : SlalTheEndlessPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<Slimed>(),
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var original = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            c => c.IsTransformable,
            this
        )).FirstOrDefault();

        if (Owner.Creature.CombatState is null) return;

        if (original is not null)
        {
            await CardCmd.Transform(original, Owner.Creature.CombatState.CreateCard<Slimed>(Owner));
        }
        await PowerCmd.Apply<SlimedFreeThisTurn>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }
}
