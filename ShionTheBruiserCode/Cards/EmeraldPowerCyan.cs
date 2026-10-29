using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Play a copy of ALL the Power cards you played this combat.
/// Whenever you draw this card, reduce its cost by 1 this combat. Exhaust. (Costs 2 less)
/// Shuffled into the Draw Pile by the Cyan Chaos Emerald.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerCyan() : ShionTheBruiserCard(4,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("PowersAmount").WithMultiplier((card, _) => GetPowersPlayed(card).Count),
    ];

    private static List<CardModel> GetPowersPlayed(CardModel card)
    {
        if (card.CombatState is null) return [];
        return CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.IsFirstInSeries && e.CardPlay.Card.Type == CardType.Power)
            .Select(e => e.CardPlay.Card)
            .ToList();
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this) EnergyCost.AddThisCombat(-1, reduceOnly: true);
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        // Listed first, so the copies played here aren't copied again
        foreach (var original in GetPowersPlayed(card))
        {
            if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead) break;

            var copy = card.CombatState.CloneCard(original);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, card.Owner);
            await CardCmd.AutoPlay(choiceContext, copy, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-2);
    }
}
