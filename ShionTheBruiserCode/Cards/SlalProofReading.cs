using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 energy and draw 1 card per Flashback you have. Lose all Flashback. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalProofReading() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("FlashbackAmount").WithMultiplier((card, _) => SlalMechanicsHelper.GetFlashback(card.Owner.Creature)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var flashback = SlalMechanicsHelper.GetFlashback(card.Owner.Creature);
        if (flashback <= 0) return;

        await PlayerCmd.GainEnergy(flashback, card.Owner);
        await CardPileCmd.Draw(choiceContext, flashback, card.Owner);
        await SlalMechanicsHelper.LoseAllFlashback(card.Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
