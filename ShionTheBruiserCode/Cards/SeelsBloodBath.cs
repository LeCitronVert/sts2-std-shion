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
/// Blood Bend X. Gain 6X (6 + 6X) Block, all at once (Dexterity applies once). Gain 2 Blur. Exhaust.
/// </summary>
public class SeelsBloodBath() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6M, ValueProp.Move),
        new DynamicVar("Bonus", 0M),
        new PowerVar<BlurPower>(2M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("BlockAmount").WithMultiplier((card, _) =>
            PreviewHelper.ModifiedBlock(card, TotalBlock(card, SeelsMechanicsHelper.PreviewBloodBendX(card)), card.DynamicVars.Block.Props)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<BlurPower>(),
    ];

    /// <summary>6 Block per X, plus 6 more per Bonus (upgrade).</summary>
    private static decimal TotalBlock(CardModel card, int x) =>
        card.DynamicVars.Block.BaseValue * (x + card.DynamicVars["Bonus"].IntValue);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        var block = TotalBlock(this, x);
        if (block > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, block, DynamicVars.Block.Props, play);
        }
        await PowerCmd.Apply<BlurPower>(choiceContext, Owner.Creature, DynamicVars["BlurPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1M);
    }
}
