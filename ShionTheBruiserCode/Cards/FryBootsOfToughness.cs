using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 10 Block. Gain 3 (5) Bad Faith. Lose 2 (4) Damage Debt.
/// </summary>
public class FryBootsOfToughness() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10M, ValueProp.Move),
        new PowerVar<BadFaith>(3M),
        new DynamicVar("DebtLoss", 2M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BadFaith>(),
        HoverTipFactory.FromPower<DamageDebt>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await CreatureCmd.GainBlock(creature, DynamicVars.Block, play);
        await FryMechanicsHelper.GainBadFaith(choiceContext, creature, DynamicVars["BadFaith"].BaseValue, this);
        await FryMechanicsHelper.LoseDamageDebt(choiceContext, creature, DynamicVars["DebtLoss"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BadFaith"].UpgradeValueBy(2M);
        DynamicVars["DebtLoss"].UpgradeValueBy(2M);
    }
}
