using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 10 Block. Request 1. Decrease ALL your Requested counters by 1.
/// </summary>
public class FryScottyTime() : FryTheCommanderCard(1, //todo : choose cost
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10M, ValueProp.Move),
        new DynamicVar("Request", 1M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, ctx =>
            FryMechanicsHelper.DecreaseAllRequests(ctx, Owner.Creature, 1));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3M); //todo : upgrade not specified
    }
}
