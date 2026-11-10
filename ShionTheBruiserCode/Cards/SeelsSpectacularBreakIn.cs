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
/// Wounding. Add 1 Glass 2 Slash (Slash+) into your Hand.
/// Add 1 more on top of your Draw Pile for each Wound in your Hand.
/// </summary>
public class SeelsSpectacularBreakIn() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int GlassAmount = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Wounding];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("SlashesAmount").WithMultiplier((card, _) => SeelsMechanicsHelper.PreviewWoundsInHandAfterWounding(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Wound>(),
        HoverTipFactory.FromCard<SeelsSlash>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Glass>(GlassAmount),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await SeelsMechanicsHelper.AddWounds(Owner, 1);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(CreateGlassSlashes(DynamicVars.Cards.IntValue), PileType.Hand, Owner));

        // Counted once the Slash is in the Hand (it isn't a Wound, so it doesn't change the count).
        var amount = SeelsMechanicsHelper.GetWoundsInHand(Owner).Count();
        if (amount > 0)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(CreateGlassSlashes(amount), PileType.Draw, Owner, CardPilePosition.Top));
        }
    }

    private List<CardModel> CreateGlassSlashes(int amount) =>
        Enumerable.Range(0, amount).Select(_ =>
        {
            var slash = SeelsMechanicsHelper.CreateSlash(Owner, IsUpgraded);
            SeelsMechanicsHelper.Enchant<Glass>(slash, GlassAmount);
            return slash;
        }).ToList();

    protected override void OnUpgrade()
    {
    }
}
