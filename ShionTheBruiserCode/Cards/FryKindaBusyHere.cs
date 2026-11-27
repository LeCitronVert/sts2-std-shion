using BaseLib.Utils;
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
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 5 Block. Gain 5 (6) additional Block for each Requested counter you have.
/// </summary>
public class FryKindaBusyHere() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5M, ValueProp.Move),
        new DynamicVar("BlockPerCounter", 5M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // Block is gained once per Requested counter, so modifiers apply to each gain
        new CalculatedVar("BlockAmount").WithMultiplier((card, _) =>
            FryMechanicsHelper.GetRequests(card.Owner.Creature).Count
            * PreviewHelper.ModifiedBlock(card, card.DynamicVars["BlockPerCounter"].BaseValue, card.DynamicVars.Block.Props)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Request)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        var count = FryMechanicsHelper.GetRequests(Owner.Creature).Count;
        for (var i = 0; i < count; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["BlockPerCounter"].BaseValue, DynamicVars.Block.Props, play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerCounter"].UpgradeValueBy(1M); //todo : sheet only says "6 block"
    }
}
