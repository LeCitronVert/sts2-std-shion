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
/// Event card. Erratic. Deal 1 damage to ALL enemies. Whenever you draw this card, increase its damage by 1 for each of your
/// Erratic cards and draw 1 additional card. Whenever you end your turn with this card in your Hand, Gamble your Hand
/// (this card included). Eternal. Upgrade: costs 0, loses Eternal.
/// </summary>
public class SeelsGamblingAddiction() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Event,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Erratic, CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1M, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ErraticAmount").WithMultiplier((card, _) => CountErraticCards(card)),
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

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    // Erratic from the start: the Seels tracker (which randomizes Erratic costs on draw) may not exist yet
    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this || Owner.Creature.HasPower<SeelsTracker>()) return;
        await SeelsMechanicsHelper.RandomizeErraticCost(choiceContext, this);
        SeelsMechanicsHelper.EnsureCreatureHasSeelsTracker(Owner.Creature);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this || CombatState is null) return;
        var increase = CountErraticCards(this);
        if (increase > 0) DynamicVars.Damage.BaseValue += increase;
        await CardPileCmd.Draw(choiceContext, 1, Owner);
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
        RemoveKeyword(CardKeyword.Eternal);
    }
}
