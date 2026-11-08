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
/// Exhaust ALL your Wounds. Gain 1 Blood per enchanted card Exhausted this way. Gloat 2. Exhaust.
/// </summary>
public class SeelsShedThePain() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BloodPerCard", 1M),
        new PowerVar<Gloat>(2M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("BloodAmount").WithMultiplier((card, _) =>
            GetWoundsToExhaust(card).Count(c => c.Enchantment is not null) * card.DynamicVars["BloodPerCard"].BaseValue),
    ];

    private static List<CardModel> GetWoundsToExhaust(CardModel card) =>
        SeelsMechanicsHelper.GetAllCombatCards(card.Owner)
            .Where(c => SeelsMechanicsHelper.IsWound(c) && c.Pile?.Type != PileType.Exhaust)
            .ToList();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Wound>(),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<Gloat>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var wounds = GetWoundsToExhaust(this);
        // Counted before Exhausting, in case Exhausting removes an enchantment.
        var enchanted = wounds.Count(c => c.Enchantment is not null);
        foreach (var wound in wounds)
        {
            await CardCmd.Exhaust(choiceContext, wound);
        }

        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner.Creature, enchanted * DynamicVars["BloodPerCard"].BaseValue, this);
        await SeelsMechanicsHelper.Gloat(choiceContext, Owner.Creature, DynamicVars["Gloat"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
