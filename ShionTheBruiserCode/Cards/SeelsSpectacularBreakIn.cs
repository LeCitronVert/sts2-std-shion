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
/// Blood Bend X. Add X (X+1) Glass 2 Slashes into your Draw Pile.
/// </summary>
public class SeelsSpectacularBreakIn() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int GlassAmount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 0M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromCard<SeelsSlash>(),
        ..HoverTipFactory.FromEnchantment<Glass>(GlassAmount),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this) + DynamicVars["Bonus"].IntValue;
        var slashes = Enumerable.Range(0, x).Select(_ =>
        {
            var slash = SeelsMechanicsHelper.CreateSlash(Owner, false);
            SeelsMechanicsHelper.Enchant<Glass>(slash, GlassAmount);
            return slash;
        }).ToList();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(slashes, PileType.Draw, Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1M);
    }
}
