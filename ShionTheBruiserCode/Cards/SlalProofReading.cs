using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Mimic 3. Gain 1 Strength for each Flashback. Lose ALL Flashback.
/// Upgraded: costs 0.
/// (The previous version is kept commented out at the bottom of this file, in case it comes back.)
/// </summary>
public class SlalProofReading() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
        new DynamicVar("StrengthPerFlashback", 1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("StrengthAmount").WithMultiplier((card, _) =>
            SlalMechanicsHelper.GetFlashback(card.Owner.Creature) * card.DynamicVars["StrengthPerFlashback"].BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.Mimic(choiceContext, card.Owner, card.DynamicVars.Cards.IntValue);

        var flashback = SlalMechanicsHelper.GetFlashback(card.Owner.Creature);
        if (flashback <= 0) return;

        await PowerCmd.Apply<StrengthPower>(choiceContext, card.Owner.Creature, flashback * card.DynamicVars["StrengthPerFlashback"].BaseValue, card.Owner.Creature, card);
        await SlalMechanicsHelper.LoseAllFlashback(card.Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

// ── Previous Proof Reading (before Change Pending J), kept in case it's reverted ──
// Loc: "Draw cards until you draw a [gold]Novel[/gold] card.\nGain {energyPrefix:energyIcons(1)} for each [gold]Flashback[/gold].{InCombat:\n({FlashbackAmount:diff()} [gold]Flashback[/gold])|}\nLose all [gold]Flashback[/gold]."
//
// /// <summary>
// /// Draw cards until you draw a Novel card. Gain 1 energy per Flashback. Lose all Flashback. Exhaust.
// /// Upgraded: loses Exhaust and gains Always Novel.
// /// Like a normal draw, the Discard Pile is shuffled in when the Draw Pile runs out. Stops when the Hand is full.
// /// </summary>
// public class SlalProofReading() : SlalTheEndlessCard(0,
//     CardType.Skill, CardRarity.Common,
//     TargetType.Self)
// {
//     public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
//
//     protected override IEnumerable<DynamicVar> CanonicalVars => [
//         new CalculationBaseVar(0),
//         new CalculationExtraVar(1),
//         new CalculatedVar("FlashbackAmount").WithMultiplier((card, _) => SlalMechanicsHelper.GetFlashback(card.Owner.Creature)),
//     ];
//
//     protected override IEnumerable<IHoverTip> ExtraHoverTips => [
//         HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
//         HoverTipFactory.FromPower<Flashback>(),
//         HoverTipFactory.ForEnergy(this),
//     ];
//
//     protected override async Task OnPlay(
//         PlayerChoiceContext choiceContext,
//         CardPlay play)
//     {
//         var card = this;
//         while (PileType.Hand.GetPile(card.Owner).Cards.Count < CardPile.MaxCardsInHand)
//         {
//             var drawn = await CardPileCmd.Draw(choiceContext, card.Owner);
//             if (drawn is null || SlalMechanicsHelper.IsNovel(drawn)) break;
//         }
//
//         var flashback = SlalMechanicsHelper.GetFlashback(card.Owner.Creature);
//         if (flashback <= 0) return;
//
//         await PlayerCmd.GainEnergy(flashback, card.Owner);
//         await SlalMechanicsHelper.LoseAllFlashback(card.Owner.Creature);
//     }
//
//     protected override void OnUpgrade()
//     {
//         RemoveKeyword(CardKeyword.Exhaust);
//         AddKeyword(SlalCardKeywords.AlwaysNovel);
//     }
// }
