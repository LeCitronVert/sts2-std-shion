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
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Argue 6 (9). If you have Damage Debt, Argue 6 (9) again.
/// </summary>
public class FryRefugeInAudacity() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Argue", 6M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Argue),
        HoverTipFactory.FromPower<DamageDebt>(),
    ];

    protected override bool ShouldGlowGoldInternal => FryMechanicsHelper.GetDamageDebt(Owner.Creature) > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var argue = DynamicVars["Argue"].BaseValue;
        await FryMechanicsHelper.Argue(choiceContext, Owner.Creature, argue, this);
        if (FryMechanicsHelper.GetDamageDebt(Owner.Creature) > 0)
        {
            await FryMechanicsHelper.Argue(choiceContext, Owner.Creature, argue, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Argue"].UpgradeValueBy(3M);
    }
}
