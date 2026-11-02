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
/// Draw cards until you draw a Novel card. Gain 1 energy per Flashback. Lose all Flashback. Exhaust.
/// Upgraded: loses Exhaust and gains Always Novel.
/// Like a normal draw, the Discard Pile is shuffled in when the Draw Pile runs out. Stops when the Hand is full.
/// </summary>
public class SlalProofReading() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("FlashbackAmount").WithMultiplier((card, _) => SlalMechanicsHelper.GetFlashback(card.Owner.Creature)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        while (PileType.Hand.GetPile(card.Owner).Cards.Count < CardPile.MaxCardsInHand)
        {
            var drawn = await CardPileCmd.Draw(choiceContext, card.Owner);
            if (drawn is null || SlalMechanicsHelper.IsNovel(drawn)) break;
        }

        var flashback = SlalMechanicsHelper.GetFlashback(card.Owner.Creature);
        if (flashback <= 0) return;

        await PlayerCmd.GainEnergy(flashback, card.Owner);
        await SlalMechanicsHelper.LoseAllFlashback(card.Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
        AddKeyword(SlalCardKeywords.AlwaysNovel);
    }
}
