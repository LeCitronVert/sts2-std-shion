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
/// Wounding. Make ALL your Wounds playable for 1 energy. All Wounds in your Hand are enchanted with Swift 3 (4).
/// </summary>
public class SeelsEyeOpeningRealization() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int WoundCost = 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Wounding];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Swift", 3M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Wound>(),
        ..HoverTipFactory.FromEnchantment<Swift>(DynamicVars["Swift"].IntValue),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await SeelsMechanicsHelper.AddWounds(Owner, 1);
        SeelsMechanicsHelper.MakeAllWoundsPlayable(Owner, WoundCost);
        SeelsMechanicsHelper.EnchantWoundsInHand<Swift>(Owner, DynamicVars["Swift"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Swift"].UpgradeValueBy(1M);
    }
}
