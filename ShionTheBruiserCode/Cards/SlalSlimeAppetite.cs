using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Each time you create a Slimed, gain 4 (6) Block. Whenever you play a Slimed, gain 1 (2) Dexterity this turn.
/// </summary>
public class SlalSlimeAppetite() : SlalTheEndlessCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SlimeAppetite>(4),
        new DynamicVar("Dexterity", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<DexterityPower>(),
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
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SlimeAppetite"].UpgradeValueBy(2);
        DynamicVars["Dexterity"].UpgradeValueBy(1);
    }
}
