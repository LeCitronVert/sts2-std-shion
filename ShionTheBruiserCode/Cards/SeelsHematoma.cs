using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 7 (9) Block. Transform any number of cards in your Hand into Swift 1 Wounds. They are free to play this turn
/// (see <see cref="HematomaWounds"/>).
/// </summary>
public class SeelsHematoma() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(7M, ValueProp.Move),
        new DynamicVar("Swift", 1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Wound>(),
        ..HoverTipFactory.FromEnchantment<Swift>(DynamicVars["Swift"].IntValue),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
        if (handCount == 0 || CombatState is null) return;

        var chosen = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, handCount),
            null,
            this
        )).ToList();
        if (chosen.Count == 0) return;

        List<CardModel> wounds = [];
        foreach (var original in chosen)
        {
            var wound = CombatState.CreateCard<Wound>(Owner);
            await CardCmd.Transform(original, wound);
            SeelsMechanicsHelper.Enchant<Swift>(wound, DynamicVars["Swift"].BaseValue);
            wounds.Add(wound);
        }

        // A second Hematoma this turn adds to the same (hidden) power
        var power = Owner.Creature.GetPower<HematomaWounds>()
                    ?? await PowerCmd.Apply<HematomaWounds>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        power?.AddWounds(wounds);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
