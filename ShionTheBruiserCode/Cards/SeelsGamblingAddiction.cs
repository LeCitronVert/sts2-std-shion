using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Event card. For each of your Erratic cards, deal damage equal to this card's energy cost to a random enemy.
/// At the end of your turn, if this is in your Hand, Gamble your Hand (this card included). Exhaust. Eternal. Upgrade: costs 0.
/// </summary>
public class SeelsGamblingAddiction() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Event,
    TargetType.RandomEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DamageAmount").WithMultiplier((card, _) =>
            card.CombatState is null ? 0 : card.EnergyCost.GetAmountToSpend()),
        new CalculatedVar("Hits").WithMultiplier((card, _) => CountErraticCards(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
    ];

    private static int CountErraticCards(CardModel card)
    {
        if (card.CombatState is null) return 0;
        return SeelsMechanicsHelper.GetAllCombatCards(card.Owner).Count(SeelsMechanicsHelper.IsErratic);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        // The energy cost it was played for (0 if it was made free).
        var damage = play.Resources.EnergyValue;
        var hits = CountErraticCards(this);
        if (damage <= 0 || hits <= 0) return;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Unpowered()
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    // Not HasTurnEndInHandEffect/OnTurnEndInHand: the game moves the card to the Play pile before that hook and to
    // the Discard pile after it, so the card would never Gamble itself (and would be pulled back off the Draw Pile).
    // BeforeFlush runs at end of turn while the Hand is still intact, before it's discarded.
    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || CombatState is null) return;
        // Another copy may already have Gambled this one away.
        if (Pile?.Type != PileType.Hand) return;
        await SeelsMechanicsHelper.GambleHand(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
