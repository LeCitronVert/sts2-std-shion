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
/// Gloat 2. Add 1 Inky Slash (Slash+) into your Hand, and 2 more into your Draw Pile per energy this card costs.
/// </summary>
public class SeelsPinThemDown() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Gloat>(2M),
        new CardsVar(1),
        new DynamicVar("SlashesPerEnergy", 2M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("SlashesAmount").WithMultiplier((card, _) =>
            GetDrawPileSlashes(card)),
    ];

    private static int GetDrawPileSlashes(CardModel card) =>
        card.EnergyCost.GetResolved() * card.DynamicVars["SlashesPerEnergy"].IntValue;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Gloat>(),
        HoverTipFactory.FromCard<SeelsSlash>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Inky>(),
    ];

    private List<CardModel> CreateInkySlashes(int amount) =>
        Enumerable.Range(0, amount).Select(_ =>
        {
            var slash = SeelsMechanicsHelper.CreateSlash(Owner, IsUpgraded);
            SeelsMechanicsHelper.Enchant<Inky>(slash, 1);
            return slash;
        }).ToList();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // Read before anything can change the cost.
        var drawPileSlashes = GetDrawPileSlashes(this);
        await SeelsMechanicsHelper.Gloat(choiceContext, Owner.Creature, DynamicVars["Gloat"].BaseValue, this);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(CreateInkySlashes(DynamicVars.Cards.IntValue), PileType.Hand, Owner));
        if (drawPileSlashes > 0)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(CreateInkySlashes(drawPileSlashes), PileType.Draw, Owner, CardPilePosition.Random));
        }
    }

    protected override void OnUpgrade()
    {
    }
}
