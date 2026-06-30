using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionMightyGuard() : ShionTheBruiserCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MightyGuard>(1),
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<MightyGuard>(choiceContext, card.Owner.Creature, DynamicVars["MightyGuard"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MightyGuard"].UpgradeValueBy(1);
    }
}