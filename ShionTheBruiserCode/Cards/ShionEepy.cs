using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEepy() : ShionTheBruiserCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PatchUpVar(999),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HealAmount").WithMultiplier((card, _) =>
            ShionMechanicsHelper.GetPatchUpHealAmount(card.DynamicVars[PatchUpVar.Key].IntValue, card.Owner.Creature))
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await ShionMechanicsHelper.PatchUpCreature(DynamicVars[PatchUpVar.Key].IntValue, card.Owner.Creature);
        ShionVfx.PlayEepy(card.Owner.Creature);
        PlayerCmd.EndTurn(card.Owner, false);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}