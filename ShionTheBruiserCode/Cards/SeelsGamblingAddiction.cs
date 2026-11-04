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
/// Ethereal. Gain 1 energy. Lose 3 (1) HP.
/// Whenever you Gamble X, put this card into your Hand (not from the Exhaust Pile).
/// If this card is Exhausted, deal 5 damage to ALL enemies for each time you Gambled this combat.
/// </summary>
public class SeelsGamblingAddiction() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self), ISeelsListener
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new HpLossVar(3M),
        new DynamicVar("DamagePerGamble", 5M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DamageAmount").WithMultiplier((card, _) =>
            card.CombatState is null ? 0 : SeelsMechanicsHelper.GetGambleCount(card.Owner.Creature) * card.DynamicVars["DamagePerGamble"].IntValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, this);
    }

    public async Task AfterGamble(PlayerChoiceContext choiceContext, Player player, int amount, IReadOnlyList<CardModel> cards)
    {
        if (player != Owner || Pile is null) return;
        if (Pile.Type is not (PileType.Draw or PileType.Discard)) return;
        await CardPileCmd.Add(this, PileType.Hand);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card != this || CombatState is null) return;
        var damage = SeelsMechanicsHelper.GetGambleCount(Owner.Creature) * DynamicVars["DamagePerGamble"].BaseValue;
        var enemies = CombatState.HittableEnemies.ToList();
        if (damage <= 0 || enemies.Count == 0) return;
        await CreatureCmd.Damage(choiceContext, enemies, damage, ValueProp.Unpowered, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-2M);
    }
}
