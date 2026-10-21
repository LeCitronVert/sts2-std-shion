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
/// Request 3 (2). Remove your Damage Debt. Lose ALL Weak, Frail and Vulnerable.
/// </summary>
public class FryExpendableClone() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override int CanonicalStarCost => 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Request", 3M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DamageDebt>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            await FryMechanicsHelper.RemoveDamageDebt(ctx, creature, this);
            await PowerCmd.Remove<WeakPower>(creature);
            await PowerCmd.Remove<FrailPower>(creature);
            await PowerCmd.Remove<VulnerablePower>(creature);
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Request"].UpgradeValueBy(-1M);
    }
}
