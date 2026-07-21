using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionNap() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    public const int PatchUpMultiplier = 3;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("XBonus", 0),
        new DynamicVar("PatchUpMultiplier", PatchUpMultiplier)
    ];
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var xAmount = card.ResolveEnergyXValue() * PatchUpMultiplier;
        if (xAmount == 0) return;
        await CreatureCmd.Heal(
            card.Owner.Creature,
            xAmount
        );
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}