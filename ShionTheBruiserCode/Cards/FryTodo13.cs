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
/// Request 2. The Valkyrs grant 10 (12) Block and 10 (12) Block next turn.
/// </summary>
public class FryTodo13() : FryTheCommanderCard(1, //todo : choose cost
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request, FryCardKeywords.Valkyrs];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Request", 2M),
        new BlockVar(10M, ValueProp.Unpowered),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlockNextTurnPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            await FryMechanicsHelper.ValkyrsBlock(creature, DynamicVars.Block.BaseValue);
            await PowerCmd.Apply<BlockNextTurnPower>(ctx, creature, DynamicVars.Block.BaseValue, creature, this);
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
