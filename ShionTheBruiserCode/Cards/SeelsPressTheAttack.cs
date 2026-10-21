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
/// Add a Momentum 5 Slash (Slash+) into your Hand. It loses Exhaust and gains Erratic.
/// </summary>
public class SeelsPressTheAttack() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int MomentumAmount = 5;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<Momentum>(MomentumAmount),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var slash = SeelsMechanicsHelper.CreateSlash(Owner, IsUpgraded);
        SeelsMechanicsHelper.Enchant<Momentum>(slash, MomentumAmount);
        slash.RemoveKeyword(CardKeyword.Exhaust);
        SeelsMechanicsHelper.AddErratic(slash);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(slash, PileType.Hand, Owner));
    }

    protected override void OnUpgrade()
    {
    }
}
