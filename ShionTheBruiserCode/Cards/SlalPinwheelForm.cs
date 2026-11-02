using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Token power created by Creativity Unbound.
/// Whenever you Shapeshift or Mimic, apply 4 Confusion to ALL enemies, and gain the created card's cost.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalPinwheelForm() : SlalTheEndlessCard(0,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PinwheelForm>(4),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<PinwheelForm>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["PinwheelForm"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        // Doubles the stacks.
        DynamicVars["PinwheelForm"].UpgradeValueBy(4);
    }
}
