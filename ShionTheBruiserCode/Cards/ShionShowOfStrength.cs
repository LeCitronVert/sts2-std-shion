using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionShowOfStrength() : ShionTheBruiserCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    public const int BonusRate = 10;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ShowOfStrength>(1),
        new DynamicVar("BonusRate", BonusRate),
        new CritDisplayTooltipVar()
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        await PowerCmd.Apply<CritRateUp>(choiceContext, card.Owner.Creature, BonusRate, card.Owner.Creature, card);
        await PowerCmd.Apply<ShowOfStrength>(choiceContext, card.Owner.Creature, DynamicVars["ShowOfStrength"].IntValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ShowOfStrength"].UpgradeValueBy(1);
    }
}