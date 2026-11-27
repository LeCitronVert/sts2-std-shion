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
/// Gain 5 Block. Argue 3 (5). If you have Damage Debt, double this card's Block and Argue.
/// </summary>
public class FryBootsOfToughness() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5M, ValueProp.Move),
        new DynamicVar("Argue", 3M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Argue),
        HoverTipFactory.FromPower<DamageDebt>(),
    ];

    protected override bool ShouldGlowGoldInternal => FryMechanicsHelper.GetDamageDebt(Owner.Creature) > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // Checked once, before the Argue can pay the debt off
        var times = FryMechanicsHelper.GetDamageDebt(Owner.Creature) > 0 ? 2 : 1;
        for (var i = 0; i < times; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        }
        await FryMechanicsHelper.Argue(choiceContext, Owner.Creature, DynamicVars["Argue"].BaseValue * times, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Argue"].UpgradeValueBy(2M); //todo : upgrade not specified (sheet still says "5 Bad Faith, 4 Damage Debt")
    }
}
