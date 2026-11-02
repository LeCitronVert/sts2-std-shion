using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Goofball Token Power. The first 1 (2) time(s) you play a Power each turn, ALL enemies lose 1 Strength this turn.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalVentilatorForm() : SlalTheEndlessCard(0,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VentilatorForm>(1),
        new DynamicVar("StrengthLoss", VentilatorForm.StrengthLoss),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<VentilatorForm>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["VentilatorForm"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VentilatorForm"].UpgradeValueBy(1);
    }
}
