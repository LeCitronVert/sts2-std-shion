using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Walking Encyclopedia Form: whenever you play a non-Novel card from your Hand, gain 1 Random.
/// </summary>
public class SlalWalkingEncyclopedia() : SlalTheEndlessCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<EncyclopediaForm>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<EncyclopediaForm>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["EncyclopediaForm"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
