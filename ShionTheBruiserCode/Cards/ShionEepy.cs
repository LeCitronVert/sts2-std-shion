using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEepy() : ShionTheBruiserCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(20),
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
        CardKeyword.Exhaust,
    ];


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.Heal(card.Owner.Creature, card.DynamicVars.Heal.BaseValue);
        PlayerCmd.EndTurn(card.Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(10);
    }
}