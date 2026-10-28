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

[Pool(typeof(TokenCardPool))]
public class SlalOctopusForm() : SlalTheEndlessCard(1,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<OctopusForm>(1),
        new PowerVar<DexterityPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars.Dexterity.BaseValue,
            card.Owner.Creature,
            card
        );
        await PowerCmd.Apply<OctopusForm>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["OctopusForm"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OctopusForm"].UpgradeValueBy(1);
    }
}
