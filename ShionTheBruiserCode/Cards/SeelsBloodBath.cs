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
/// Blood Bend X. Gain 5 Block X (X+1) times. Also add X Slashes into your Discard Pile.
/// </summary>
public class SeelsBloodBath() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5M, ValueProp.Move),
        new DynamicVar("Bonus", 0M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Block is gained X times, so modifiers apply to each gain
        new CalculatedVar("BlockAmount").WithMultiplier((card, _) =>
            (SeelsMechanicsHelper.PreviewBloodBendX(card) + card.DynamicVars["Bonus"].IntValue)
            * PreviewHelper.ModifiedBlock(card, card.DynamicVars.Block.BaseValue, card.DynamicVars.Block.Props)),
        new CalculatedVar("SlashesAmount").WithMultiplier((card, _) => SeelsMechanicsHelper.PreviewBloodBendX(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromCard<SeelsSlash>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        var times = x + DynamicVars["Bonus"].IntValue;
        for (var i = 0; i < times; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        }
        await SeelsMechanicsHelper.AddSlashes(Owner, x, PileType.Discard, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1M);
    }
}
