using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Each time you create a Slimed, gain 4 (5) Block. Whenever you play a Slimed, gain 1 Dexterity this turn.
/// Whenever you play a Slimy card, apply 1 Weak to a random enemy and gain 1 Plating.
/// </summary>
public class SlalSlimeAppetite() : SlalTheEndlessCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SlimeAppetite>(4),
        new DynamicVar("Dexterity", 1),
        new DynamicVar("SlimyBonus", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var power = await PowerCmd.Apply<SlimeAppetite>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["SlimeAppetite"].BaseValue,
            card.Owner.Creature,
            card
        );
        power?.AddToSecondaryValue("Dexterity", card.DynamicVars["Dexterity"].BaseValue);
        power?.AddToSecondaryValue("SlimyBonus", card.DynamicVars["SlimyBonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SlimeAppetite"].UpgradeValueBy(1);
    }
}
