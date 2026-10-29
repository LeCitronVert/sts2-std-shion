using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Thorn Ring's card. You can't heal or revive during this fight.
/// Change all your cards that aren't Non-Native or interact with Non-Native cards into random Non-Native cards.
/// At the start of your turn, lose 1 HP, gain 1 Energy and draw 1 card.
/// </summary>
public class ShionComeWhatMay() : ShionTheBruiserCard(0,
    CardType.Power, CardRarity.Ancient,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ComeWhatMay>(1),
        new HpLossVar(1),
        new EnergyVar(1),
        new CardsVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.NonNative),
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner;
        await PowerCmd.Apply<ComeWhatMay>(choiceContext, owner.Creature, DynamicVars["ComeWhatMay"].BaseValue, owner.Creature, card);

        var options = ShionMechanicsHelper.GetNonNativeCardOptions(owner).ToList();
        if (options.Count == 0) return;

        foreach (var original in ShionMechanicsHelper.GetAllCombatCards(owner)
                     .Where(c => c.IsTransformable
                                 && ShionMechanicsHelper.IsCardNativeToCharacter(c, owner.Character)
                                 && !ShionMechanicsHelper.InteractsWithNonNative(c))
                     .ToList())
        {
            await CardCmd.Transform(
                new CardTransformation(original, options).Yield(),
                owner.RunState.Rng.CombatCardSelection
            );
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
