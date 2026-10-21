using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalSarcophagusForm() : SlalTheEndlessCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SarcophagusForm>(2),
        new EnergyVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var power = await PowerCmd.Apply<SarcophagusForm>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["SarcophagusForm"].BaseValue,
            card.Owner.Creature,
            card
        );
        power?.AddToSecondaryValue("Energy", card.DynamicVars.Energy.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SarcophagusForm"].UpgradeValueBy(2);
    }
}
