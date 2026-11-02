using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Token power created by Creativity Unbound.
/// Walking Encyclopedia Form: whenever you play a non-Novel card from your Hand, gain 1 Random.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalWalkingEncyclopedia() : SlalTheEndlessCard(0,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    public override bool CanBeGeneratedInCombat => false;

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
        // Doubles the stacks.
        DynamicVars["EncyclopediaForm"].UpgradeValueBy(1);
    }
}
