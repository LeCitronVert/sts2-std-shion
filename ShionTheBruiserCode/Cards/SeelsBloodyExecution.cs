using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
/// Blood Bend X. Add 2 Sharp X Slashes (Slashes+) into your Hand and 2 into your Draw Pile.
/// </summary>
public class SeelsBloodyExecution() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("SharpAmount").WithMultiplier((card, _) => SeelsMechanicsHelper.PreviewBloodBendX(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromCard<SeelsSlash>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Sharp>(),
    ];

    private List<CardModel> CreateSharpSlashes(int amount, int sharp)
    {
        return Enumerable.Range(0, amount).Select(_ =>
        {
            var slash = SeelsMechanicsHelper.CreateSlash(Owner, IsUpgraded);
            if (sharp > 0) SeelsMechanicsHelper.Enchant<Sharp>(slash, sharp);
            return slash;
        }).ToList();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        var amount = DynamicVars.Cards.IntValue;

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(CreateSharpSlashes(amount, x), PileType.Hand, Owner));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(CreateSharpSlashes(amount, x), PileType.Draw, Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
    }
}
