using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 6 Bad Faith. Lose half of your Damage Debt.
/// </summary>
public class FryRefugeInAudacity() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<BadFaith>(6M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DebtLossAmount").WithMultiplier((card, _) => FryMechanicsHelper.GetDamageDebt(card.Owner.Creature) / 2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BadFaith>(),
        HoverTipFactory.FromPower<DamageDebt>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FryMechanicsHelper.GainBadFaith(choiceContext, Owner.Creature, DynamicVars["BadFaith"].BaseValue, this);
        await FryMechanicsHelper.LoseHalfDamageDebt(choiceContext, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BadFaith"].UpgradeValueBy(3M); //todo : upgrade not specified
    }
}
