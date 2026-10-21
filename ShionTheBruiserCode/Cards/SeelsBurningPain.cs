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
/// Wounding. ALL Wounds in your Hand lose Unplayable and are enchanted with Sown.
/// </summary>
public class SeelsBurningPain() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public const int SownAmount = 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Wounding];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Wound>(),
        ..HoverTipFactory.FromEnchantment<Sown>(SownAmount),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await SeelsMechanicsHelper.AddWounds(Owner, 1);
        SeelsMechanicsHelper.EmpowerWoundsInHand<Sown>(Owner, SownAmount);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
