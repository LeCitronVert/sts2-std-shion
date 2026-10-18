using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionPressTheAdvantage() : CritContributorCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int BonusRate = 10;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PressTheAdvantage>(3),
        new DynamicVar("BonusRate", BonusRate),
        new CritDisplayTooltipVar(),
        new PatchUpVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        await PowerCmd.Apply<CritRateUp>(choiceContext, card.Owner.Creature, BonusRate, card.Owner.Creature, card);
        await PowerCmd.Apply<PressTheAdvantage>(choiceContext, card.Owner.Creature, DynamicVars["PressTheAdvantage"].IntValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PressTheAdvantage"].UpgradeValueBy(2);
        DynamicVars[PatchUpVar.Key].UpgradeValueBy(2);
    }
}