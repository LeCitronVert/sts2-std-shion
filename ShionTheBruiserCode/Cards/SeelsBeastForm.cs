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
/// Whenever you play an Attack, gain 2 Strength this turn. Ethereal. Upgrade: costs 2.
/// </summary>
public class SeelsBeastForm() : SeelsTheRecklessCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BeastForm>(2M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<BeastForm>(choiceContext, Owner.Creature, DynamicVars["BeastForm"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
