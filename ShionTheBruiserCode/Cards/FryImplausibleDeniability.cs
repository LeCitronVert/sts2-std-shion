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
/// Argue equal to your Block. Lose your Block. (Upgrade: then, if you have Damage Debt, Argue that much again.)
/// </summary>
public class FryImplausibleDeniability() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override int CanonicalStarCost => 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ArgueAmount").WithMultiplier((card, _) => card.Owner.Creature.Block),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Argue),
        HoverTipFactory.FromPower<DamageDebt>(),
    ];

    protected override bool ShouldGlowGoldInternal => Owner.Creature.Block > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        var amount = creature.Block;
        if (amount <= 0) return;

        await FryMechanicsHelper.Argue(choiceContext, creature, amount, this);
        await CreatureCmd.LoseBlock(choiceContext, creature, amount, creature);
        if (IsUpgraded && FryMechanicsHelper.GetDamageDebt(creature) > 0)
        {
            await FryMechanicsHelper.Argue(choiceContext, creature, amount, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
